using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prokat.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Инкремент к уже существующей схеме (см. «Основной запрос.sql»): колонки + таблица аренды.
            migrationBuilder.Sql(@"
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
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.[Аренда_бронирование]', N'U') IS NULL
   AND OBJECT_ID(N'dbo.[Счет_оплаты]', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.[Инвентарь]', N'U') IS NOT NULL
BEGIN
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
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.[Аренда_бронирование]', N'U') IS NOT NULL
    DROP TABLE dbo.[Аренда_бронирование];
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.[Счет_оплаты]', N'БазоваяСумма') IS NOT NULL
    ALTER TABLE dbo.[Счет_оплаты] DROP COLUMN [БазоваяСумма];
");
        }
    }
}
