using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prokat.Migrations
{
    /// <inheritdoc />
    public partial class SplitInventoryDomainTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ботинки",
                table: "Инвентарь");

            migrationBuilder.DropColumn(
                name: "Лыжи",
                table: "Инвентарь");

            migrationBuilder.DropColumn(
                name: "Маска",
                table: "Инвентарь");

            migrationBuilder.DropColumn(
                name: "Одежда",
                table: "Инвентарь");

            migrationBuilder.DropColumn(
                name: "Палки",
                table: "Инвентарь");

            migrationBuilder.DropColumn(
                name: "Сноуборд",
                table: "Инвентарь");

            migrationBuilder.DropColumn(
                name: "Шлем",
                table: "Инвентарь");

            migrationBuilder.AddColumn<int>(
                name: "ID_Ботинки",
                table: "Инвентарь",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_Лыжи",
                table: "Инвентарь",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_Очки",
                table: "Инвентарь",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_Палки",
                table: "Инвентарь",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_Сноуборд",
                table: "Инвентарь",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_Шлем",
                table: "Инвентарь",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Ботинки",
                columns: table => new
                {
                    ID_Ботинки = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Название = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Тип = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    РазмерEU = table.Column<int>(type: "int", nullable: false),
                    Примечание = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ботинки", x => x.ID_Ботинки);
                });

            migrationBuilder.CreateTable(
                name: "Лыжи",
                columns: table => new
                {
                    ID_Лыжи = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Название = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Тип = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    РостовкаСм = table.Column<int>(type: "int", nullable: false),
                    Уровень = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Примечание = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Лыжи", x => x.ID_Лыжи);
                });

            migrationBuilder.CreateTable(
                name: "Очки",
                columns: table => new
                {
                    ID_Очки = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Название = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Размер = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ТипЛинзы = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Очки", x => x.ID_Очки);
                });

            migrationBuilder.CreateTable(
                name: "Палки",
                columns: table => new
                {
                    ID_Палки = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Название = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Тип = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ДлинаСм = table.Column<int>(type: "int", nullable: false),
                    Примечание = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Палки", x => x.ID_Палки);
                });

            migrationBuilder.CreateTable(
                name: "Сноуборды",
                columns: table => new
                {
                    ID_Сноуборд = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Название = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Тип = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    РостовкаСм = table.Column<int>(type: "int", nullable: false),
                    Жесткость = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Примечание = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Сноуборды", x => x.ID_Сноуборд);
                });

            migrationBuilder.CreateTable(
                name: "Шлемы",
                columns: table => new
                {
                    ID_Шлем = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Название = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Размер = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Тип = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Шлемы", x => x.ID_Шлем);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Инвентарь_ID_Ботинки",
                table: "Инвентарь",
                column: "ID_Ботинки");

            migrationBuilder.CreateIndex(
                name: "IX_Инвентарь_ID_Лыжи",
                table: "Инвентарь",
                column: "ID_Лыжи");

            migrationBuilder.CreateIndex(
                name: "IX_Инвентарь_ID_Очки",
                table: "Инвентарь",
                column: "ID_Очки");

            migrationBuilder.CreateIndex(
                name: "IX_Инвентарь_ID_Палки",
                table: "Инвентарь",
                column: "ID_Палки");

            migrationBuilder.CreateIndex(
                name: "IX_Инвентарь_ID_Сноуборд",
                table: "Инвентарь",
                column: "ID_Сноуборд");

            migrationBuilder.CreateIndex(
                name: "IX_Инвентарь_ID_Шлем",
                table: "Инвентарь",
                column: "ID_Шлем");

            migrationBuilder.AddForeignKey(
                name: "FK_Инвентарь_Ботинки_ID_Ботинки",
                table: "Инвентарь",
                column: "ID_Ботинки",
                principalTable: "Ботинки",
                principalColumn: "ID_Ботинки",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Инвентарь_Лыжи_ID_Лыжи",
                table: "Инвентарь",
                column: "ID_Лыжи",
                principalTable: "Лыжи",
                principalColumn: "ID_Лыжи",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Инвентарь_Очки_ID_Очки",
                table: "Инвентарь",
                column: "ID_Очки",
                principalTable: "Очки",
                principalColumn: "ID_Очки",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Инвентарь_Палки_ID_Палки",
                table: "Инвентарь",
                column: "ID_Палки",
                principalTable: "Палки",
                principalColumn: "ID_Палки",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Инвентарь_Сноуборды_ID_Сноуборд",
                table: "Инвентарь",
                column: "ID_Сноуборд",
                principalTable: "Сноуборды",
                principalColumn: "ID_Сноуборд",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Инвентарь_Шлемы_ID_Шлем",
                table: "Инвентарь",
                column: "ID_Шлем",
                principalTable: "Шлемы",
                principalColumn: "ID_Шлем",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Инвентарь_Ботинки_ID_Ботинки",
                table: "Инвентарь");

            migrationBuilder.DropForeignKey(
                name: "FK_Инвентарь_Лыжи_ID_Лыжи",
                table: "Инвентарь");

            migrationBuilder.DropForeignKey(
                name: "FK_Инвентарь_Очки_ID_Очки",
                table: "Инвентарь");

            migrationBuilder.DropForeignKey(
                name: "FK_Инвентарь_Палки_ID_Палки",
                table: "Инвентарь");

            migrationBuilder.DropForeignKey(
                name: "FK_Инвентарь_Сноуборды_ID_Сноуборд",
                table: "Инвентарь");

            migrationBuilder.DropForeignKey(
                name: "FK_Инвентарь_Шлемы_ID_Шлем",
                table: "Инвентарь");

            migrationBuilder.DropTable(
                name: "Ботинки");

            migrationBuilder.DropTable(
                name: "Лыжи");

            migrationBuilder.DropTable(
                name: "Очки");

            migrationBuilder.DropTable(
                name: "Палки");

            migrationBuilder.DropTable(
                name: "Сноуборды");

            migrationBuilder.DropTable(
                name: "Шлемы");

            migrationBuilder.DropIndex(
                name: "IX_Инвентарь_ID_Ботинки",
                table: "Инвентарь");

            migrationBuilder.DropIndex(
                name: "IX_Инвентарь_ID_Лыжи",
                table: "Инвентарь");

            migrationBuilder.DropIndex(
                name: "IX_Инвентарь_ID_Очки",
                table: "Инвентарь");

            migrationBuilder.DropIndex(
                name: "IX_Инвентарь_ID_Палки",
                table: "Инвентарь");

            migrationBuilder.DropIndex(
                name: "IX_Инвентарь_ID_Сноуборд",
                table: "Инвентарь");

            migrationBuilder.DropIndex(
                name: "IX_Инвентарь_ID_Шлем",
                table: "Инвентарь");

            migrationBuilder.DropColumn(
                name: "ID_Ботинки",
                table: "Инвентарь");

            migrationBuilder.DropColumn(
                name: "ID_Лыжи",
                table: "Инвентарь");

            migrationBuilder.DropColumn(
                name: "ID_Очки",
                table: "Инвентарь");

            migrationBuilder.DropColumn(
                name: "ID_Палки",
                table: "Инвентарь");

            migrationBuilder.DropColumn(
                name: "ID_Сноуборд",
                table: "Инвентарь");

            migrationBuilder.DropColumn(
                name: "ID_Шлем",
                table: "Инвентарь");

            migrationBuilder.AddColumn<string>(
                name: "Ботинки",
                table: "Инвентарь",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Лыжи",
                table: "Инвентарь",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Маска",
                table: "Инвентарь",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Одежда",
                table: "Инвентарь",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Палки",
                table: "Инвентарь",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Сноуборд",
                table: "Инвентарь",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Шлем",
                table: "Инвентарь",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
