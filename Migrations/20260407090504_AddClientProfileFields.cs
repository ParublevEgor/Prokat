using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prokat.Migrations
{
    /// <inheritdoc />
    public partial class AddClientProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.[Клиент]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.[Клиент]', N'РазмерОбуви') IS NULL
        ALTER TABLE dbo.[Клиент] ADD [РазмерОбуви] INT NULL;

    IF COL_LENGTH(N'dbo.[Клиент]', N'ФотоПрофиля') IS NULL
        ALTER TABLE dbo.[Клиент] ADD [ФотоПрофиля] NVARCHAR(MAX) NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.[Клиент]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.[Клиент]', N'РазмерОбуви') IS NOT NULL
        ALTER TABLE dbo.[Клиент] DROP COLUMN [РазмерОбуви];

    IF COL_LENGTH(N'dbo.[Клиент]', N'ФотоПрофиля') IS NOT NULL
        ALTER TABLE dbo.[Клиент] DROP COLUMN [ФотоПрофиля];
END
");
        }
    }
}
