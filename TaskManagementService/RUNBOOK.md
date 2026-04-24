# RUNBOOK — TaskManagementService

Операционный runbook для локальной и стендовой эксплуатации.

## 1. Состав сервисов

- `TaskManagement.API` — CRUD по задачам, миграции БД, публикация событий.
- `TaskManagement.Observer` — прием sync HTTP и async RabbitMQ событий.
- PostgreSQL — хранилище задач.
- RabbitMQ — брокер событий (опционально, есть fallback).

---

## 2. Быстрый старт (локально)

1. Убедиться, что доступна PostgreSQL из строки `ConnectionStrings:TaskManagementDb`.
2. (Опционально) Убедиться, что доступен RabbitMQ (`RabbitMq:Host`, `RabbitMq:Port`).
3. Запустить Observer:
   - `dotnet run --project src/TaskManagement.Observer/TaskManagement.Observer.csproj`
4. Запустить API:
   - `dotnet run --project src/TaskManagement.API/TaskManagement.API.csproj`
5. Проверить Swagger:
   - `http://localhost:<api-port>/swagger`

---

## 3. Проверка готовности

Минимальный smoke:

1. `POST /api/tasks` — создать задачу.
2. `GET /api/tasks/{id}` — проверить, что задача читается.
3. `PUT /api/tasks/{id}` — сменить статус.
4. `DELETE /api/tasks/{id}` — удалить задачу.

Ожидаемое поведение:

- CRUD работает независимо от состояния RabbitMQ.
- При недоступном RabbitMQ в логах warning, но API не падает.
- Sync HTTP в Observer продолжает работать.

---

## 4. Логи и сигналы

### Нормальные сообщения

- `Now listening on: ...`
- `No migrations were applied. The database is already up to date.`

### Сообщения деградации (ожидаемые)

- `[Warning] RabbitMQ (...) is unavailable. Running without async messaging.`
- `[Warning] RabbitMQ (...) is unavailable. Running observer in HTTP-only mode.`

Это не авария, если бизнес-требование допускает работу без async-очереди.

---

## 5. Частые инциденты

## 5.1 Swagger не открывается

Проверить:

- API действительно запущен.
- правильный URL/порт (`/swagger`).
- порт не занят другим процессом.

Команда проверки процесса на порту (PowerShell):

```powershell
Get-NetTCPConnection -LocalPort <PORT> -State Listen
```

## 5.2 Ошибка подключения к БД

Проверить:

- `ConnectionStrings:TaskManagementDb`;
- доступность хоста БД и порта `5432`;
- креды пользователя;
- права на БД.

Признак в логах: ошибки `Npgsql`/`DbUpdateException`/`Timeout`.

## 5.3 RabbitMQ недоступен

Проверить:

- `RabbitMq:Host`, `RabbitMq:Port`, `Username`, `Password`;
- доступность порта;
- открыт ли AMQP-порт (обычно `5672`).

Важно:

- `15672` обычно порт web-management UI RabbitMQ, не AMQP транспорт.
- Для MassTransit/RabbitMQ-клиента обычно нужен именно `5672`.

## 5.4 Observer не получает sync HTTP события

Проверить:

- `Observer:BaseUrl` в API;
- что Observer запущен и доступен;
- endpoint `POST /api/observer/task-changed`.

---

## 6. Проверка сети (Windows, PowerShell)

Проверка TCP-доступности:

```powershell
Test-NetConnection <HOST> -Port <PORT>
```

Проверка RabbitMQ management UI:

```text
http://<HOST>:15672
```

Проверка AMQP порта для приложения:

```powershell
Test-NetConnection <HOST> -Port 5672
```

---

## 7. Режим деградации (без RabbitMQ)

Сервисы запускаются и работают:

- API:
  - CRUD активен;
  - async publish отключен;
  - sync HTTP callback остается.
- Observer:
  - HTTP endpoint активен;
  - consumer не регистрируется.

---

## 8. Чек-лист перед демонстрацией

- запускаются оба сервиса без ошибок;
- Swagger открывается;
- CRUD сценарий проходит;
- логи Observer показывают sync события;
- (если RabbitMQ доступен) есть async-сообщения в логах consumer.

---

## 9. Полезные команды

Сборка:

```bash
dotnet build TaskManagementService.slnx
```

Тесты:

```bash
dotnet test TaskManagementService.slnx
```

Запуск API:

```bash
dotnet run --project src/TaskManagement.API/TaskManagement.API.csproj
```

Запуск Observer:

```bash
dotnet run --project src/TaskManagement.Observer/TaskManagement.Observer.csproj
```

