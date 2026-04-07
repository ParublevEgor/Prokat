SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.[Аренда_бронирование]', N'U') IS NULL
OR OBJECT_ID(N'dbo.[Инвентарь]', N'U') IS NULL
OR OBJECT_ID(N'dbo.[Клиент]', N'U') IS NULL
OR OBJECT_ID(N'dbo.[Счет_оплаты]', N'U') IS NULL
BEGIN
    PRINT N'Не найдены нужные таблицы. Сначала примените скрипты создания схемы.';
    RETURN;
END;

IF OBJECT_ID(N'dbo.TRG_Аренда_Проверки', N'TR') IS NOT NULL
    DROP TRIGGER dbo.TRG_Аренда_Проверки;
IF OBJECT_ID(N'dbo.TRG_Инвентарь_ЗапретУдаления', N'TR') IS NOT NULL
    DROP TRIGGER dbo.TRG_Инвентарь_ЗапретУдаления;
IF OBJECT_ID(N'dbo.TRG_Клиент_ЗапретУдаления', N'TR') IS NOT NULL
    DROP TRIGGER dbo.TRG_Клиент_ЗапретУдаления;
GO

--1 Проверка дат аренды
CREATE TRIGGER dbo.TRG_Аренда_Проверки
ON dbo.[Аренда_бронирование]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE [ДатаОкончания] <= [ДатаНачала]
    )
    BEGIN
        RAISERROR(N'Дата окончания должна быть позже даты начала.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        JOIN dbo.[Аренда_бронирование] AS r
            ON r.[ID_Инвентаря] = i.[ID_Инвентаря]
           AND r.[ID_Аренды] <> i.[ID_Аренды]
        WHERE i.[Статус] <> N'Отмена'
          AND r.[Статус] <> N'Отмена'
          AND r.[ДатаНачала] < i.[ДатаОкончания]
          AND i.[ДатаНачала] < r.[ДатаОкончания]
    )
    BEGIN
        RAISERROR(N'Пересечение интервалов аренды для одного инвентаря.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;
END;
GO

--2 Запрет на удаление инвентаря в работе
CREATE TRIGGER dbo.TRG_Инвентарь_ЗапретУдаления
ON dbo.[Инвентарь]
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM deleted AS d
        JOIN dbo.[Аренда_бронирование] AS r ON r.[ID_Инвентаря] = d.[ID_Инвентаря]
        WHERE r.[Статус] IN (N'Бронь', N'Выдано')
    )
    BEGIN
        RAISERROR(N'Нельзя удалить инвентарь: есть активные бронирования или выдачи.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;
END;
GO

-- Запрет на удаление клиента с открытым счётом оплаты
CREATE TRIGGER dbo.TRG_Клиент_ЗапретУдаления
ON dbo.[Клиент]
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM deleted AS d
        JOIN dbo.[Счет_оплаты] AS s ON s.[ID_Клиента] = d.[ID_Клиента]
    )
    BEGIN
        RAISERROR(N'Нельзя удалить клиента: есть связанные счета оплаты.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;
END;
GO

PRINT N'Триггеры созданы: TRG_Аренда_Проверки, TRG_Инвентарь_ЗапретУдаления, TRG_Клиент_ЗапретУдаления.';
GO
