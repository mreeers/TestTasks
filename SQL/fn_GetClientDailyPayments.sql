USE [TestTask]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_GetClientDailyPayments]    Script Date: 24.04.2026 18:51:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   FUNCTION [dbo].[fn_GetClientDailyPayments]
(
    @ClientId BIGINT,
    @StartDate DATE,
    @EndDate DATE
)
RETURNS TABLE
AS
RETURN
(
    -- 1. Генератор чисел (Tally Table) для быстрого создания дат без ограничений рекурсии
    WITH
    L0 AS (SELECT 1 AS c UNION ALL SELECT 1),               -- 2 строки
    L1 AS (SELECT 1 AS c FROM L0 a CROSS JOIN L0 b),        -- 4 строки
    L2 AS (SELECT 1 AS c FROM L1 a CROSS JOIN L1 b),        -- 16 строк
    L3 AS (SELECT 1 AS c FROM L2 a CROSS JOIN L2 b),        -- 256 строк
    L4 AS (SELECT 1 AS c FROM L3 a CROSS JOIN L3 b),        -- 65 536 строк (этого хватит на ~179 лет)
    
    -- 2. Ограничиваем количество чисел разницей в днях между датами
    Nums AS (
        SELECT TOP (ISNULL(DATEDIFF(DAY, @StartDate, @EndDate) + 1, 0))
            ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) - 1 AS n
        FROM L4
    ),
    
    -- 3. Прибавляем числа к начальной дате, получая непрерывный календарь
    DateSeries AS (
        SELECT DATEADD(DAY, n, @StartDate) AS PaymentDate
        FROM Nums
    )
    
    -- 4. Соединяем календарь с платежами клиента
    SELECT 
        d.PaymentDate AS [Date],
        ISNULL(SUM(cp.Amount), 0) AS TotalAmount
    FROM DateSeries d
    LEFT JOIN ClientPayments cp 
        ON cp.ClientId = @ClientId
        AND cp.Dt >= d.PaymentDate 
        AND cp.Dt < DATEADD(DAY, 1, d.PaymentDate) 
    GROUP BY 
        d.PaymentDate
);
