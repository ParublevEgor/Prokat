using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prokat.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountsAndSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.[Настройки_проката]', N'U') IS NULL
   AND OBJECT_ID(N'dbo.[Клиент]', N'U') IS NOT NULL
BEGIN
    CREATE TABLE dbo.[Настройки_проката] (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [СтавкаНДС] DECIMAL(5,4) NOT NULL CONSTRAINT [DF_Настройки_СтавкаНДС] DEFAULT (0.18),
        CONSTRAINT [PK_Настройки_проката] PRIMARY KEY ([ID])
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.[Учетная_запись]', N'U') IS NULL
   AND OBJECT_ID(N'dbo.[Клиент]', N'U') IS NOT NULL
BEGIN
    CREATE TABLE dbo.[Учетная_запись] (
        [ID_Учетной_записи] INT IDENTITY(1,1) NOT NULL,
        [Логин] NVARCHAR(64) NOT NULL,
        [ПарольХеш] NVARCHAR(256) NOT NULL,
        [Роль] NVARCHAR(16) NOT NULL,
        [ID_Клиента] INT NULL,
        CONSTRAINT [PK_Учетная_запись] PRIMARY KEY ([ID_Учетной_записи]),
        CONSTRAINT [FK_Учетная_запись_Клиент_ID_Клиента] FOREIGN KEY ([ID_Клиента])
            REFERENCES dbo.[Клиент]([ID_Клиента]) ON DELETE SET NULL
    );
    CREATE UNIQUE INDEX [IX_Учетная_запись_Логин] ON dbo.[Учетная_запись]([Логин]);
    CREATE INDEX [IX_Учетная_запись_ID_Клиента] ON dbo.[Учетная_запись]([ID_Клиента]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.[Учетная_запись]', N'U') IS NOT NULL
    DROP TABLE dbo.[Учетная_запись];
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.[Настройки_проката]', N'U') IS NOT NULL
    DROP TABLE dbo.[Настройки_проката];
");
        }
    }
}
