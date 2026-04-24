# TaskManagementService

Полное описание работы сервиса управления задачами на `.NET 8`.

## Быстрый старт

Этот раздел нужен, чтобы любой разработчик мог поднять проект с нуля за 10-15 минут.

### 1) Что нужно установить

- Docker (Docker Desktop на Windows/macOS или Docker Engine на Linux).
- Docker Compose v2 (`docker compose`).
- Git.

Проверка:

```bash
docker --version
docker compose version
git --version
```

### 2) Клонирование и переход в проект

```bash
git clone <URL_вашего_репозитория>
cd TaskManagementService
```

### 3) Запуск всего стека

```bash
docker compose up -d --build
```

Что поднимется:

- `postgres` (БД),
- `rabbitmq` (брокер + UI),
- `taskmanagement-api`,
- `taskmanagement-observer`.

### 4) Проверка, что все запущено

```bash
docker compose ps
```

Ожидается статус `Up`/`Healthy` у контейнеров.

### 5) Куда заходить после старта

- Swagger API: `http://localhost:5000/swagger`
- Observer: `http://localhost:5080`
- RabbitMQ UI: `http://localhost:15672` (`proxmox/proxmox`)

### 6) Минимальная проверка функционала (smoke test)

1. Открой Swagger.
2. Выполни `POST /api/tasks` (создай задачу).
3. Выполни `GET /api/tasks` (убедись, что задача есть).
4. Выполни `PUT /api/tasks/{id}` (смени `status`).
5. Выполни `DELETE /api/tasks/{id}`.
6. Проверь логи Observer:

```bash
docker compose logs --tail=200 observer
```

В логах должны быть записи о событиях изменения задач (sync/async).

### 7) Остановка

```bash
docker compose down
```

С удалением данных в томах:

```bash
docker compose down -v
```

### 8) Если не запускается

- Проверь занятость портов `5000`, `5080`, `5432`, `5672`, `15672`.
- Если проект разворачивается на другой машине/сервере, обязательно укажи свои параметры подключения к RabbitMQ:
  - `RabbitMq:Host`
  - `RabbitMq:Port` (обычно `5672` для AMQP, `15672` только для UI)
  - `RabbitMq:Username`
  - `RabbitMq:Password`
- Для такого сценария также проверь `Observer:BaseUrl` в API-конфиге (должен указывать на адрес Observer в сети этой машины).
- При проблемах со сборкой очисти кэш:

```bash
docker builder prune -af
docker system prune -af --volumes
```

- При ошибке `No space left on device` увеличь диск хоста/LXC и перезапусти сборку.

## 1. Что это за проект

Проект состоит из двух сервисов:

- `TaskManagement.API` — основной Web API для CRUD операций с задачами.
- `TaskManagement.Observer` — сервис-наблюдатель, который получает события:
  - синхронно по HTTP от API;
  - асинхронно через RabbitMQ (MassTransit consumer).

Также проект использует:

- PostgreSQL + EF Core (Code First, миграции);
- Swagger (документация и ручное тестирование API);
- FluentValidation (валидация входных DTO);
- xUnit + Moq + FluentAssertions (unit-тесты);
- RabbitMQ (сообщения о событиях задач) с fallback-режимом при недоступности брокера.

---

## 2. Структура решения

- `src/TaskManagement.API`
  - контроллеры, DTO, валидаторы, сервисы приложения, Swagger, конфигурация.
- `src/TaskManagement.Domain`
  - доменные сущности, enum-ы, интерфейсы репозиториев, контракт интеграционного события.
- `src/TaskManagement.Infrastructure`
  - `DbContext`, EF-конфигурации, миграции, репозиторий на EF Core, DI-расширения.
- `src/TaskManagement.Observer`
  - HTTP endpoint наблюдателя + MassTransit consumer для RabbitMQ.
- `tests/TaskManagement.Tests`
  - unit-тесты бизнес-логики смены статуса задачи.

---

## 3. Доменная модель

Сущность `TaskItem` содержит:

- `Id: Guid`
- `Title: string`
- `Description: string`
- `Status: TaskStatus` (`New`, `InProgress`, `Completed`)
- `CreatedAt: DateTime`
- `UpdatedAt: DateTime`

### Поведение timestamp-ов

В `TaskManagementDbContext` переопределены `SaveChanges/SaveChangesAsync`:

- при добавлении задачи автоматически ставятся `CreatedAt` и `UpdatedAt`;
- при изменении задачи обновляется `UpdatedAt`.

---

## 4. Работа API (TaskManagement.API)

### 4.1 CRUD endpoints

Базовый маршрут: `api/tasks`

- `GET /api/tasks` — получить все задачи.
- `GET /api/tasks/{id}` — получить задачу по `id`.
- `POST /api/tasks` — создать задачу.
- `PUT /api/tasks/{id}` — обновить задачу (включая статус).
- `DELETE /api/tasks/{id}` — удалить задачу (hard delete).

### 4.2 DTO и валидация

Используются DTO:

- `CreateTaskRequest`
- `UpdateTaskRequest`
- `TaskResponse`

FluentValidation проверяет:

- `Title` — не пустой, максимум 200 символов;
- `Description` — не пустой, максимум 2000 символов.

При невалидных данных API возвращает `400 Bad Request`.

### 4.3 Сервисный слой

`TaskService` реализует use-case логику:

- чтение/создание/обновление/удаление задач через `ITaskRepository`;
- отправка уведомлений о событиях через `ITaskEventNotifier`.

---

## 5. Интеграции и события

Для изменений задач используется единый контракт события:

- `TaskChangedEvent`
  - `Id`
  - `EventType` (`Created`, `Updated`, `Deleted`)
  - `OccurredAt`

### 5.1 Синхронная интеграция (HTTP)

После create/update/delete API вызывает Observer:

- `POST /api/observer/task-changed`

Observer логирует событие в формате вида:

- `[Sync Event Log]: Task {TaskId} updated at {Time}. EventType={EventType}`

### 5.2 Асинхронная интеграция (RabbitMQ)

API публикует `TaskChangedEvent` в RabbitMQ через MassTransit.
Observer подписывается consumer-ом `TaskChangedEventConsumer` и пишет лог:

- `[Event Log]: Task {TaskId} updated at {Time}. EventType={EventType}`

---

## 6. Fallback при недоступном RabbitMQ

Реализована проверка доступности RabbitMQ перед регистрацией транспорта.

### Как это работает

1. На старте сервиса выполняется TCP probe до `RabbitMq:Host` и `RabbitMq:Port`.
2. Если RabbitMQ доступен:
   - включается MassTransit + RabbitMQ transport.
3. Если RabbitMQ недоступен:
   - сервис не падает;
   - выводится warning в лог;
   - API продолжает работать без async-публикации (используется `NullTaskEventPublisher`);
   - sync HTTP уведомления в Observer продолжают работать.

Это позволяет продолжать CRUD-операции даже при временной недоступности брокера.

---

## 7. База данных и миграции (Code First)

Используется EF Core Code First.

- Контекст: `TaskManagementDbContext`
- Миграции находятся в:
  - `src/TaskManagement.Infrastructure/Persistence/Migrations`

При старте `TaskManagement.API` вызывается:

- `dbContext.Database.Migrate();`

Это автоматически применяет миграции к базе (если есть непримененные).

---

## 8. Конфигурация

### 8.1 API (`src/TaskManagement.API/appsettings.json`)

- `ConnectionStrings:TaskManagementDb` — строка подключения PostgreSQL.
- `RabbitMq:Host`
- `RabbitMq:Port`
- `RabbitMq:Username`
- `RabbitMq:Password`
- `Observer:BaseUrl` — URL Observer для sync HTTP callback.

Важно: при разворачивании не на вашей локальной машине (другой ПК/LXC/VM/сервер) значения `RabbitMq:*` и `Observer:BaseUrl` нужно заменить на реальные адреса и учетные данные целевого окружения.

### 8.2 Observer (`src/TaskManagement.Observer/appsettings.json`)

- `RabbitMq:Host`
- `RabbitMq:Port`
- `RabbitMq:Username`
- `RabbitMq:Password`

Важно: для другого окружения обязательно укажи собственные параметры RabbitMQ (`Host/Port/Username/Password`), иначе Observer не сможет подключиться к брокеру.

---

## 9. Swagger

Swagger включен постоянно, не только в `Development`.

Открыть UI:

- `http://localhost:<api-port>/swagger`

JSON-схема:

- `/swagger/v1/swagger.json`

---

## 9.1 RabbitMQ GUI (вариант 1 для Observer)

Для GUI используется встроенный RabbitMQ Management UI (отдельный веб-интерфейс брокера):

- URL: `http://192.168.1.228:15672`
- Логин/пароль: `proxmox/proxmox`

Важно:

- `RabbitMq:Port` в `appsettings` для API/Observer должен быть `5672` (AMQP-порт клиента).
- `15672` используется только для веб-интерфейса мониторинга RabbitMQ.

В UI можно смотреть:

- очереди и их глубину;
- активных consumers;
- входящие/исходящие сообщения;
- состояние соединений и каналов.

---

## 10. Локальный запуск

### Вариант 1: запуск API

```bash
dotnet run --project src/TaskManagement.API/TaskManagement.API.csproj
```

### Вариант 2: запуск Observer

```bash
dotnet run --project src/TaskManagement.Observer/TaskManagement.Observer.csproj
```

### Вариант 3: запуск API + Observer одним скриптом (Windows PowerShell)

```powershell
.\scripts\run-local.ps1
```

Скрипт поднимает:

- `Observer` на `http://localhost:5080`
- `API` на `http://localhost:5000`

### Вариант 4: полный локальный запуск (тесты -> Observer -> API)

```powershell
.\scripts\run-all.ps1
```

Сценарий:

1. Запускает unit-тесты.
2. При успешных тестах поднимает `Observer`.
3. После этого запускает `API`.

Если тесты не проходят, сервисы не запускаются.

---

## 11. Тестирование

Запуск unit-тестов:

```bash
dotnet test TaskManagementService.slnx
```

Покрытие unit-тестами (`tests/TaskManagement.Tests`):

- `TaskService`:
  - создание задачи + отправка события `Created`;
  - обновление статуса + отправка события `Updated`;
  - удаление задачи + отправка события `Deleted`;
  - поведение при отсутствии задачи для `Update`/`Delete`.
- Валидаторы:
  - `CreateTaskRequestValidator` (пустой title, слишком длинное description);
  - `UpdateTaskRequestValidator` (валидный запрос, пустой description).

---

## 12. Краткий end-to-end сценарий

1. Клиент вызывает `POST /api/tasks`.
2. API создает запись в PostgreSQL.
3. API отправляет:
   - async `TaskChangedEvent` в RabbitMQ (если доступен);
   - sync POST в Observer.
4. Observer:
   - принимает HTTP событие и логирует;
   - принимает RabbitMQ событие (если транспорт активен) и логирует.
5. При `PUT /api/tasks/{id}` аналогично идет обновление с `EventType=Updated`.
6. При `DELETE /api/tasks/{id}` задача удаляется физически (`hard delete`) с `EventType=Deleted`.

---

## 13. Основные технические решения

- Четкое разделение на слои: Domain / Infrastructure / API / Observer.
- Repository-подход для доступа к данным.
- XML-документация на публичных типах и методах production-кода.
- Защита от деградации инфраструктуры: приложение работает без RabbitMQ.

---

## 14. Что можно добавить дальше (опционально)

- Dockerfile для обоих сервисов и `docker-compose.yml`.
- Централизованный сбор логов (например, через Seq/Loki).
- Health-check endpoints (`/health`, `/health/rabbitmq`, `/health/db`).
- Интеграционные тесты API.

---

## 15. Serilog

В приложении используется `Serilog` как основной логгер для `TaskManagement.API` и `TaskManagement.Observer`.

Конфигурация задается в `appsettings.json` (секция `Serilog`):

- минимальный уровень логов;
- переопределения уровней для `Microsoft*`;
- вывод в консоль с шаблоном.

---

## 16. Полный запуск всего стека (одной командой)

Добавлен общий `docker-compose.yml`, который поднимает:

- `PostgreSQL`
- `RabbitMQ` (+ Management UI)
- `TaskManagement.API`
- `TaskManagement.Observer`

### Команда запуска

```bash
docker compose up -d --build
```

### Как проверить локально на ПК (шаг за шагом)

1. Убедиться, что Docker Desktop запущен.
2. В корне проекта выполнить:

```bash
docker compose up -d --build
```

3. Проверить статус контейнеров:

```bash
docker compose ps
```

4. Проверить, что API отвечает:

```bash
curl http://localhost:5000/swagger
```

5. Проверить работу CRUD через Swagger:

- открыть `http://localhost:5000/swagger`;
- выполнить `POST`, `GET`, `PUT`, `DELETE` для `/api/tasks`.

6. Проверить Observer:

```bash
docker compose logs observer
```

В логах должны быть записи про sync/async события изменений задач.

### Доступные URL

- API (Swagger): `http://localhost:5000/swagger`
- Observer: `http://localhost:5080`
- RabbitMQ UI: `http://localhost:15672` (`proxmox/proxmox`)

### Остановка

```bash
docker compose down
```

Для остановки с удалением volume-данных:

```bash
docker compose down -v
```

---

## 17. Логи Observer в файл

`Observer` пишет логи в файл по пути:

- `logs/<yyyy-MM-dd>/observer.log`

Например:

- `logs/2026-04-24/observer.log`

Для отдельной сборки Observer можно использовать скрипт:

```powershell
.\scripts\build-observer.ps1
```

