-- Ручное обновление существующей БД (если не используете dotnet ef database update).
-- Выполнить после скрипта «Основной запрос.sql».
--
-- Создаёт таблицу dbo.[Аренда_бронирование] (если её ещё нет). Без неё скрипт 02_Triggers.sql не выполнится.

IF OBJECT_ID(N'dbo.[Счет_оплаты]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.[Счет_оплаты]', N'БазоваяСумма') IS NULL
        ALTER TABLE dbo.[Счет_оплаты] ADD [БазоваяСумма] DECIMAL(12,2) NULL;

    IF EXISTS (
        SELECT 1 FROM sys.columns c
        JOIN sys.types t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = OBJECT_ID(N'dbo.[Счет_оплаты]')
          AND c.name = N'Сумма_оплаты' AND t.name = N'int')
    BEGIN
        ALTER TABLE dbo.[Счет_оплаты] ALTER COLUMN [Сумма_оплаты] DECIMAL(12,2) NULL;
    END
END
GO

IF OBJECT_ID(N'dbo.[Аренда_бронирование]', N'U') IS NULL
   AND OBJECT_ID(N'dbo.[Счет_оплаты]', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.[Инвентарь]', N'U') IS NOT NULL
BEGIN
    PRINT N'Создание таблицы dbo.[Аренда_бронирование]...';
    CREATE TABLE dbo.[Аренда_бронирование] (
        [ID_Аренды] INT IDENTITY(1,1) NOT NULL,
        [ID_Заказа] INT NOT NULL,
        [ID_Инвентаря] INT NOT NULL,
        [ДатаНачала] DATETIME2 NOT NULL,
        [ДатаОкончания] DATETIME2 NOT NULL,
        [Статус] NVARCHAR(20) NOT NULL,
        CONSTRAINT [PK_Аренда_бронирование] PRIMARY KEY ([ID_Аренды]),
        CONSTRAINT [FK_Аренда_бронирование_Счет_оплаты] FOREIGN KEY ([ID_Заказа])
            REFERENCES dbo.[Счет_оплаты]([ID_Заказа]),
        CONSTRAINT [FK_Аренда_бронирование_Инвентарь] FOREIGN KEY ([ID_Инвентаря])
            REFERENCES dbo.[Инвентарь]([ID_Инвентаря])
    );
    CREATE INDEX [IX_Аренда_бронирование_ID_Заказа] ON dbo.[Аренда_бронирование]([ID_Заказа]);
    CREATE INDEX [IX_Аренда_бронирование_ID_Инвентаря] ON dbo.[Аренда_бронирование]([ID_Инвентаря]);
    PRINT N'Таблица dbo.[Аренда_бронирование] создана.';
END
ELSE IF OBJECT_ID(N'dbo.[Аренда_бронирование]', N'U') IS NULL
BEGIN
    PRINT N'Пропуск создания Аренда_бронирование: нужны таблицы Счет_оплаты и Инвентарь. Сначала выполните «Основной запрос.sql».';
END
GO
