SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

--1 Расчёт стоимости заказа
--Расчитать сумму из Цены_на_услуги по длительности аренды и типу дня (будни/выходные), опционально НДС; 
--записывать итог один раз из базовой формулы, без повторного умножения при повторном EXEC.
CREATE OR ALTER PROCEDURE dbo.sp_РассчитатьСтоимостьЗаказа
    @ID_Заказа INT,
    @Сумма_НДС DECIMAL(10,4) = 0.18
AS
BEGIN
    SET NOCOUNT ON;
    SET DATEFIRST 1;

    DECLARE @Начало DATETIME2, @Конец DATETIME2;
    DECLARE @Часы INT;
    DECLARE @ТарифЧасов INT;
    DECLARE @База INT;
    DECLARE @Итог DECIMAL(12,2);

    SELECT TOP (1)
        @Начало = a.ДатаНачала,
        @Конец = a.ДатаОкончания
    FROM dbo.[Аренда_бронирование] AS a
    WHERE a.ID_Заказа = @ID_Заказа
    ORDER BY a.ID_Аренды;

    IF @Начало IS NULL OR @Конец IS NULL OR @Конец <= @Начало
    BEGIN
        RAISERROR(N'Нет корректной аренды для заказа.', 16, 1);
        RETURN;
    END;

    SET @Часы = CASE WHEN CEILING(DATEDIFF(MINUTE, @Начало, @Конец) / 60.0) < 1 THEN 1
                     ELSE CAST(CEILING(DATEDIFF(MINUTE, @Начало, @Конец) / 60.0) AS INT) END;

    SELECT TOP (1) @ТарифЧасов = t.Время_аренды
    FROM dbo.[Цены_на_услуги] AS t
    WHERE t.Время_аренды >= @Часы
    ORDER BY t.Время_аренды ASC;

    IF @ТарифЧасов IS NULL
        SELECT TOP (1) @ТарифЧасов = t.Время_аренды FROM dbo.[Цены_на_услуги] AS t ORDER BY t.Время_аренды DESC;

    IF DATEPART(WEEKDAY, @Начало) IN (6, 7)
        SELECT @База = t.Прокат_выходные_и_праздничные_дни FROM dbo.[Цены_на_услуги] AS t WHERE t.Время_аренды = @ТарифЧасов;
    ELSE
        SELECT @База = t.Прокат_будни FROM dbo.[Цены_на_услуги] AS t WHERE t.Время_аренды = @ТарифЧасов;

    IF @База IS NULL OR @База <= 0
    BEGIN
        RAISERROR(N'Не задана цена проката в тарифе.', 16, 1);
        RETURN;
    END;

    SET @Итог = CAST(@База AS DECIMAL(12,2)) * (1 + @Сумма_НДС);

    UPDATE dbo.[Счет_оплаты]
    SET БазоваяСумма = CAST(@База AS DECIMAL(12,2)),
        Сумма_оплаты = ROUND(@Итог, 2)
    WHERE ID_Заказа = @ID_Заказа;

    SELECT ID_Заказа, CAST(Сумма_оплаты AS DECIMAL(12,2)) AS ИтоговаяСумма
    FROM dbo.[Счет_оплаты]
    WHERE ID_Заказа = @ID_Заказа;
END
GO

--2 Найти свободный инвентарь
--Возвращает инвентарь, не пересекающийся с существующими бронями/арендами в переданном интервале [@Начало, @Конец] + фильтр по типу (лыжи/сноуборд).
CREATE OR ALTER PROCEDURE dbo.sp_НайтиСвободныйИнвентарь
    @ТипИнвентаря NVARCHAR(50) = NULL,
    @ДатаНачала DATETIME2,
    @ДатаОкончания DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    IF @ДатаОкончания <= @ДатаНачала
        RETURN;

    SELECT
        i.ID_Инвентаря,
        i.Лыжи,
        i.Палки,
        i.Сноуборд,
        i.Ботинки
    FROM dbo.[Инвентарь] AS i
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.[Аренда_бронирование] AS r
        WHERE r.ID_Инвентаря = i.ID_Инвентаря
          AND r.Статус <> N'Отмена'
          AND r.ДатаНачала < @ДатаОкончания
          AND @ДатаНачала < r.ДатаОкончания
    )
    AND (
        @ТипИнвентаря IS NULL OR LTRIM(RTRIM(@ТипИнвентаря)) = N''
        OR (@ТипИнвентаря = N'Лыжи' AND i.Лыжи IS NOT NULL AND i.Лыжи <> N'')
        OR (@ТипИнвентаря = N'Сноуборд' AND i.Сноуборд IS NOT NULL AND i.Сноуборд <> N'')
    )
    ORDER BY i.ID_Инвентаря;
END
GO

--3 Отчёт по клиентам
-- Использовать @ДатаНачала / @ДатаОкончания в JOIN с новой таблицей аренды
CREATE OR ALTER PROCEDURE dbo.sp_ОтчетПоКлиентам
    @ДатаНачала DATE = NULL,
    @ДатаОкончания DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        к.ID_Клиента,
        к.Фамилия + N' ' + к.Имя AS ФИО,
        к.Возраст,
        с.ID_Заказа,
        с.Сумма_оплаты,
        CASE WHEN к.Залог > 0 THEN N'Залог внесен' ELSE N'Залог не требуется' END AS СтатусЗалога,
        a.ID_Аренды,
        a.ДатаНачала,
        a.ДатаОкончания
    FROM dbo.[Клиент] AS к
    LEFT JOIN dbo.[Счет_оплаты] AS с ON к.ID_Заказа = с.ID_Заказа
    LEFT JOIN dbo.[Аренда_бронирование] AS a ON a.ID_Заказа = с.ID_Заказа
    WHERE (
        @ДатаНачала IS NULL OR a.ДатаОкончания IS NULL OR CAST(a.ДатаОкончания AS DATE) >= @ДатаНачала
    )
    AND (
        @ДатаОкончания IS NULL OR a.ДатаНачала IS NULL OR CAST(a.ДатаНачала AS DATE) <= @ДатаОкончания
    )
    ORDER BY с.Сумма_оплаты DESC;
END
GO
