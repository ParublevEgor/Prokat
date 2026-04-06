-- Триггеры проката.
-- Имена таблиц и столбцов задаются через NCHAR(...) — так скрипт не зависит от кодировки файла (избегаем Msg 8197).
-- Сообщения и имена триггеров — на русском.
-- Перед запуском: 00_VerifyRentalTable.sql; при отсутствии таблицы — 00_SchemaUpdate.sql и обновить список таблиц в SSMS.

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @tabRent sysname =
    NCHAR(0x0410)+NCHAR(0x0440)+NCHAR(0x0435)+NCHAR(0x043D)+NCHAR(0x0434)+NCHAR(0x0430)+NCHAR(0x005F)+
    NCHAR(0x0431)+NCHAR(0x0440)+NCHAR(0x043E)+NCHAR(0x043D)+NCHAR(0x0438)+NCHAR(0x0440)+NCHAR(0x043E)+
    NCHAR(0x0432)+NCHAR(0x0430)+NCHAR(0x043D)+NCHAR(0x0438)+NCHAR(0x0435);

DECLARE @tabInv sysname =
    NCHAR(0x0418)+NCHAR(0x043D)+NCHAR(0x0432)+NCHAR(0x0435)+NCHAR(0x043D)+NCHAR(0x0442)+NCHAR(0x0430)+NCHAR(0x0440)+NCHAR(0x044C);

DECLARE @tabClient sysname =
    NCHAR(0x041A)+NCHAR(0x043B)+NCHAR(0x0438)+NCHAR(0x0435)+NCHAR(0x043D)+NCHAR(0x0442);

DECLARE @tabOrder sysname =
    NCHAR(0x0421)+NCHAR(0x0447)+NCHAR(0x0435)+NCHAR(0x0442)+NCHAR(0x005F)+NCHAR(0x043E)+NCHAR(0x043F)+NCHAR(0x043B)+NCHAR(0x0430)+NCHAR(0x0442)+NCHAR(0x044B);

DECLARE @cStart sysname =
    NCHAR(0x0414)+NCHAR(0x0430)+NCHAR(0x0442)+NCHAR(0x0430)+NCHAR(0x041D)+NCHAR(0x0430)+NCHAR(0x0447)+NCHAR(0x0430)+NCHAR(0x043B)+NCHAR(0x0430);

DECLARE @cEnd sysname =
    NCHAR(0x0414)+NCHAR(0x0430)+NCHAR(0x0442)+NCHAR(0x0430)+NCHAR(0x041E)+NCHAR(0x043A)+NCHAR(0x043E)+NCHAR(0x043D)+NCHAR(0x0447)+NCHAR(0x0430)+NCHAR(0x043D)+NCHAR(0x0438)+NCHAR(0x044F);

DECLARE @cStatus sysname =
    NCHAR(0x0421)+NCHAR(0x0442)+NCHAR(0x0430)+NCHAR(0x0442)+NCHAR(0x0443)+NCHAR(0x0441);

DECLARE @cIdInv sysname =
    NCHAR(0x0049)+NCHAR(0x0044)+NCHAR(0x005F)+NCHAR(0x0418)+NCHAR(0x043D)+NCHAR(0x0432)+NCHAR(0x0435)+NCHAR(0x043D)+NCHAR(0x0442)+NCHAR(0x0430)+NCHAR(0x0440)+NCHAR(0x044F);

DECLARE @cIdRent sysname =
    NCHAR(0x0049)+NCHAR(0x0044)+NCHAR(0x005F)+NCHAR(0x0410)+NCHAR(0x0440)+NCHAR(0x0435)+NCHAR(0x043D)+NCHAR(0x0434)+NCHAR(0x044B);

DECLARE @cIdClient sysname =
    NCHAR(0x0049)+NCHAR(0x0044)+NCHAR(0x005F)+NCHAR(0x041A)+NCHAR(0x043B)+NCHAR(0x0438)+NCHAR(0x0435)+NCHAR(0x043D)+NCHAR(0x0442)+NCHAR(0x0430);

DECLARE @sCancel nvarchar(20) =
    NCHAR(0x041E)+NCHAR(0x0442)+NCHAR(0x043C)+NCHAR(0x0435)+NCHAR(0x043D)+NCHAR(0x0430);

DECLARE @sBron nvarchar(20) =
    NCHAR(0x0411)+NCHAR(0x0440)+NCHAR(0x043E)+NCHAR(0x043D)+NCHAR(0x044C);

DECLARE @sVidano nvarchar(20) =
    NCHAR(0x0412)+NCHAR(0x044B)+NCHAR(0x0434)+NCHAR(0x0430)+NCHAR(0x043D)+NCHAR(0x043E);

DECLARE @onRent nvarchar(520) = QUOTENAME(N'dbo') + N'.' + QUOTENAME(@tabRent);
DECLARE @onInv nvarchar(520) = QUOTENAME(N'dbo') + N'.' + QUOTENAME(@tabInv);
DECLARE @onClient nvarchar(520) = QUOTENAME(N'dbo') + N'.' + QUOTENAME(@tabClient);
DECLARE @onOrder nvarchar(520) = QUOTENAME(N'dbo') + N'.' + QUOTENAME(@tabOrder);

IF OBJECT_ID(@onRent, N'U') IS NULL
   OR OBJECT_ID(@onInv, N'U') IS NULL
   OR OBJECT_ID(@onClient, N'U') IS NULL
   OR OBJECT_ID(@onOrder, N'U') IS NULL
BEGIN
    PRINT N'Не найдена одна из таблиц в базе [' + DB_NAME() + N']. Выполните здесь 00_SchemaUpdate.sql и «Основной запрос.sql», затем 00_VerifyRentalTable.sql.';
    PRINT N'Ожидаемое полное имя таблицы аренды: ' + @onRent;
    RETURN;
END;

-- Старые имена (латиница), если остались после предыдущей версии скрипта
IF OBJECT_ID(N'dbo.TRG_Rental_Check', N'TR') IS NOT NULL DROP TRIGGER dbo.TRG_Rental_Check;
IF OBJECT_ID(N'dbo.TRG_Inventory_BlockDelete', N'TR') IS NOT NULL DROP TRIGGER dbo.TRG_Inventory_BlockDelete;
IF OBJECT_ID(N'dbo.TRG_Client_BlockDelete', N'TR') IS NOT NULL DROP TRIGGER dbo.TRG_Client_BlockDelete;

IF OBJECT_ID(N'dbo.TRG_Аренда_Проверки', N'TR') IS NOT NULL DROP TRIGGER dbo.TRG_Аренда_Проверки;
IF OBJECT_ID(N'dbo.TRG_Инвентарь_ЗапретУдаления', N'TR') IS NOT NULL DROP TRIGGER dbo.TRG_Инвентарь_ЗапретУдаления;
IF OBJECT_ID(N'dbo.TRG_Клиент_ЗапретУдаления', N'TR') IS NOT NULL DROP TRIGGER dbo.TRG_Клиент_ЗапретУдаления;

DECLARE @sql1 nvarchar(max) = N'
CREATE TRIGGER dbo.TRG_Аренда_Проверки ON ' + @onRent + N'
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE ' + QUOTENAME(@cEnd) + N' <= ' + QUOTENAME(@cStart) + N')
    BEGIN
        RAISERROR(N''Дата окончания должна быть позже даты начала.'', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        INNER JOIN ' + @onRent + N' AS r
            ON r.' + QUOTENAME(@cIdInv) + N' = i.' + QUOTENAME(@cIdInv) + N'
           AND r.' + QUOTENAME(@cIdRent) + N' <> i.' + QUOTENAME(@cIdRent) + N'
        WHERE i.' + QUOTENAME(@cStatus) + N' <> N''' + @sCancel + N'''
          AND r.' + QUOTENAME(@cStatus) + N' <> N''' + @sCancel + N'''
          AND r.' + QUOTENAME(@cStart) + N' < i.' + QUOTENAME(@cEnd) + N'
          AND i.' + QUOTENAME(@cStart) + N' < r.' + QUOTENAME(@cEnd) + N'
    )
    OR EXISTS (
        SELECT 1
        FROM inserted AS i1
        INNER JOIN inserted AS i2
            ON i1.' + QUOTENAME(@cIdInv) + N' = i2.' + QUOTENAME(@cIdInv) + N'
           AND i1.' + QUOTENAME(@cIdRent) + N' < i2.' + QUOTENAME(@cIdRent) + N'
        WHERE i1.' + QUOTENAME(@cStatus) + N' <> N''' + @sCancel + N''' AND i2.' + QUOTENAME(@cStatus) + N' <> N''' + @sCancel + N'''
          AND i1.' + QUOTENAME(@cStart) + N' < i2.' + QUOTENAME(@cEnd) + N'
          AND i2.' + QUOTENAME(@cStart) + N' < i1.' + QUOTENAME(@cEnd) + N'
    )
    BEGIN
        RAISERROR(N''Пересечение интервалов аренды для одного инвентаря.'', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END';

EXEC sp_executesql @sql1;

DECLARE @sql2 nvarchar(max) = N'
CREATE TRIGGER dbo.TRG_Инвентарь_ЗапретУдаления ON ' + @onInv + N'
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM deleted AS d
        INNER JOIN ' + @onRent + N' AS r ON r.' + QUOTENAME(@cIdInv) + N' = d.' + QUOTENAME(@cIdInv) + N'
        WHERE r.' + QUOTENAME(@cStatus) + N' IN (N''' + @sBron + N''', N''' + @sVidano + N''')
    )
    BEGIN
        RAISERROR(N''Нельзя удалить инвентарь: есть активные бронирования или выдачи.'', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END';

EXEC sp_executesql @sql2;

DECLARE @sql3 nvarchar(max) = N'
CREATE TRIGGER dbo.TRG_Клиент_ЗапретУдаления ON ' + @onClient + N'
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM deleted AS d
        INNER JOIN ' + @onOrder + N' AS s ON s.' + QUOTENAME(@cIdClient) + N' = d.' + QUOTENAME(@cIdClient) + N'
    )
    BEGIN
        RAISERROR(N''Нельзя удалить клиента: есть связанные счета оплаты.'', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END';

EXEC sp_executesql @sql3;

PRINT N'Триггеры созданы: dbo.TRG_Аренда_Проверки, dbo.TRG_Инвентарь_ЗапретУдаления, dbo.TRG_Клиент_ЗапретУдаления.';
GO
