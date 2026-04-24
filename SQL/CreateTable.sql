-- 1. Создание таблицы
CREATE TABLE ClientPayments (
    Id BIGINT PRIMARY KEY,        -- первичный ключ
    ClientId BIGINT NOT NULL,     -- Id клиента
    Dt DATETIME2(0) NOT NULL,     -- дата платежа (без долей секунды)
    Amount MONEY NOT NULL         -- сумма платежа
);
GO

-- 2. Наполнение данными
INSERT INTO ClientPayments (Id, ClientId, Dt, Amount)
VALUES 
    (1, 1, '2022-01-03 17:24:00', 100),
    (2, 1, '2022-01-05 17:24:14', 200),
    (3, 1, '2022-01-05 18:23:34', 250),
    (4, 1, '2022-01-07 10:12:38', 50),
    (5, 2, '2022-01-05 17:24:14', 278),
    (6, 2, '2022-01-10 12:39:29', 300);
GO

-- 3. Проверка результата
SELECT * FROM ClientPayments;