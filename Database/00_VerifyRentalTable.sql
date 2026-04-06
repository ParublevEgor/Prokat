-- Проверка: выполняйте в той же базе, что и 00_SchemaUpdate / 02_Triggers.
-- Имя таблицы подбирается через NCHAR(...) — без проблем с кодировкой файла.

SET NOCOUNT ON;

SELECT @@SERVERNAME AS [Сервер], DB_NAME() AS [Текущая_база];

DECLARE @rent sysname =
    NCHAR(0x0410)+NCHAR(0x0440)+NCHAR(0x0435)+NCHAR(0x043D)+NCHAR(0x0434)+NCHAR(0x0430)+NCHAR(0x005F)+
    NCHAR(0x0431)+NCHAR(0x0440)+NCHAR(0x043E)+NCHAR(0x043D)+NCHAR(0x0438)+NCHAR(0x0440)+NCHAR(0x043E)+
    NCHAR(0x0432)+NCHAR(0x0430)+NCHAR(0x043D)+NCHAR(0x0438)+NCHAR(0x0435);

DECLARE @full nvarchar(520) = QUOTENAME(N'dbo') + N'.' + QUOTENAME(@rent);

SELECT @full AS [Полное_имя_таблицы], OBJECT_ID(@full, N'U') AS [Id_объекта_USER_TABLE];

SELECT s.name AS [Схема], t.name AS [Имя_таблицы], t.type_desc AS [Тип]
FROM sys.tables AS t
JOIN sys.schemas AS s ON s.schema_id = t.schema_id
WHERE t.name = @rent;

IF OBJECT_ID(@full, N'U') IS NULL
    PRINT N'ИТОГ: таблица не найдена. Выполните в этой базе 00_SchemaUpdate.sql, затем обновите список таблиц в SSMS (ПКМ по «Таблицы» — «Обновить»).';
ELSE
    PRINT N'ИТОГ: таблица найдена.';
GO
