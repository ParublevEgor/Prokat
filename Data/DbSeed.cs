using Microsoft.EntityFrameworkCore;
using Prokat.API.Models;
using System;
using System.Data;
using System.Globalization;

namespace Prokat.API.Data
{
    public static class DbSeed
    {
        public static void EnsureDefaults(ApplicationDbContext db)
        {
            if (HasTable(db, "Настройки_проката") && !db.SiteSettings.AsNoTracking().Any())
            {
                db.SiteSettings.Add(new SiteSettings { СтавкаНДС = 0.18m });
                db.SaveChanges();
            }

            if (HasTable(db, "Учетная_запись") && !db.AppUsers.AsNoTracking().Any(u => u.Логин == "admin"))
            {
                db.AppUsers.Add(new AppUser
                {
                    Логин = "admin",
                    ПарольХеш = BCrypt.Net.BCrypt.HashPassword("12345"),
                    Роль = "Admin",
                    ID_Клиента = null,
                });
                db.SaveChanges();
            }

            if (HasTable(db, "Инвентарь"))
                SeedInventory(db);
        }

        private static bool HasTable(ApplicationDbContext db, string tableName)
        {
            var conn = db.Database.GetDbConnection();
            var needClose = conn.State != ConnectionState.Open;
            if (needClose) conn.Open();
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT OBJECT_ID(@name, 'U')";
                var p = cmd.CreateParameter();
                p.ParameterName = "@name";
                p.Value = $"dbo.[{tableName}]";
                cmd.Parameters.Add(p);
                var res = cmd.ExecuteScalar();
                return res != null && res != DBNull.Value;
            }
            finally
            {
                if (needClose) conn.Close();
            }
        }

        private static void SeedInventory(ApplicationDbContext db)
        {
            SeedCatalogs(db);

            if (db.Inventory.AsNoTracking().Any(i => i.ID_Лыжи != null || i.ID_Сноуборд != null))
                return;

            var skis = db.Skis.AsNoTracking().OrderBy(x => x.РостовкаСм).ToList();
            var boards = db.Snowboards.AsNoTracking().OrderBy(x => x.РостовкаСм).ToList();
            var boots = db.Boots.AsNoTracking().OrderBy(x => x.РазмерEU).ToList();
            var poles = db.Poles.AsNoTracking().OrderBy(x => x.ДлинаСм).ToList();
            var helmets = db.Helmets.AsNoTracking().OrderBy(x => x.Размер).ToList();
            var goggles = db.Goggles.AsNoTracking().OrderBy(x => x.Размер).ToList();

            var rows = new List<Inventory>();
            foreach (var ski in skis)
            {
                var pair = PickByLength(boots, ski.РостовкаСм);
                rows.Add(new Inventory
                {
                    ID_Лыжи = ski.ID_Лыжи,
                    ID_Палки = PickPolesForLength(poles, ski.РостовкаСм)?.ID_Палки,
                    ID_Ботинки = pair?.ID_Ботинки,
                    ID_Шлем = PickBySize(helmets, SizeClassForLength(ski.РостовкаСм))?.ID_Шлем,
                    ID_Очки = PickBySize(goggles, SizeClassForLength(ski.РостовкаСм))?.ID_Очки,
                });
            }

            foreach (var board in boards)
            {
                var pair = PickByLength(boots, board.РостовкаСм);
                rows.Add(new Inventory
                {
                    ID_Сноуборд = board.ID_Сноуборд,
                    ID_Ботинки = pair?.ID_Ботинки,
                    ID_Шлем = PickBySize(helmets, SizeClassForLength(board.РостовкаСм))?.ID_Шлем,
                    ID_Очки = PickBySize(goggles, SizeClassForLength(board.РостовкаСм))?.ID_Очки,
                });
            }

            db.Inventory.AddRange(rows);
            db.SaveChanges();
        }

        private static void SeedCatalogs(ApplicationDbContext db)
        {
            if (!db.Skis.AsNoTracking().Any())
            {
                db.Skis.AddRange(
                    new SkiItem { Название = "Atomic Bent", Тип = "Горные", РостовкаСм = 172, Уровень = "Продвинутый" },
                    new SkiItem { Название = "Rossignol XP", Тип = "Классические", РостовкаСм = 170, Уровень = "Базовый" },
                    new SkiItem { Название = "Head Shape", Тип = "Горные", РостовкаСм = 180, Уровень = "Средний" }
                );
                db.SaveChanges();
            }

            if (!db.Snowboards.AsNoTracking().Any())
            {
                db.Snowboards.AddRange(
                    new SnowboardItem { Название = "Jones Flagship", Тип = "Фрирайд", РостовкаСм = 158, Жесткость = "Средняя" },
                    new SnowboardItem { Название = "Burton Custom", Тип = "Универсальный", РостовкаСм = 162, Жесткость = "Средняя" },
                    new SnowboardItem { Название = "Lib Tech Dynamo", Тип = "Олл-маунтин", РостовкаСм = 156, Жесткость = "Мягкая" }
                );
                db.SaveChanges();
            }

            if (!db.Boots.AsNoTracking().Any())
            {
                db.Boots.AddRange(
                    new BootsItem { Название = "Atomic Hawx", Тип = "Лыжные", РазмерEU = 38 },
                    new BootsItem { Название = "Atomic Hawx", Тип = "Лыжные", РазмерEU = 41 },
                    new BootsItem { Название = "Salomon Dialogue", Тип = "Сноубордические", РазмерEU = 44 }
                );
                db.SaveChanges();
            }

            if (!db.Poles.AsNoTracking().Any())
            {
                db.Poles.AddRange(
                    new PolesItem { Название = "Leki Airfoil", Тип = "Горнолыжные", ДлинаСм = 110 },
                    new PolesItem { Название = "Scott Team", Тип = "Горнолыжные", ДлинаСм = 120 },
                    new PolesItem { Название = "Head Multi", Тип = "Горнолыжные", ДлинаСм = 130 }
                );
                db.SaveChanges();
            }

            if (!db.Helmets.AsNoTracking().Any())
            {
                db.Helmets.AddRange(
                    new HelmetItem { Название = "Giro Ledge", Размер = "S", Тип = "Универсальный" },
                    new HelmetItem { Название = "Smith Mission", Размер = "M", Тип = "Универсальный" },
                    new HelmetItem { Название = "Head Radar", Размер = "L", Тип = "Универсальный" }
                );
                db.SaveChanges();
            }

            if (!db.Goggles.AsNoTracking().Any())
            {
                db.Goggles.AddRange(
                    new GogglesItem { Название = "Anon Helix", Размер = "S", ТипЛинзы = "S2" },
                    new GogglesItem { Название = "Smith Squad", Размер = "M", ТипЛинзы = "S2" },
                    new GogglesItem { Название = "Oakley Line Miner", Размер = "L", ТипЛинзы = "S3" }
                );
                db.SaveChanges();
            }
        }

        private static string SizeClassForLength(int lengthCm)
        {
            if (lengthCm < 160) return "S";
            if (lengthCm <= 175) return "M";
            return "L";
        }

        private static BootsItem? PickByLength(List<BootsItem> boots, int lengthCm)
        {
            return lengthCm switch
            {
                < 160 => boots.OrderBy(x => Math.Abs(x.РазмерEU - 38)).FirstOrDefault(),
                <= 175 => boots.OrderBy(x => Math.Abs(x.РазмерEU - 41)).FirstOrDefault(),
                _ => boots.OrderBy(x => Math.Abs(x.РазмерEU - 44)).FirstOrDefault()
            };
        }

        private static PolesItem? PickPolesForLength(List<PolesItem> poles, int lengthCm)
        {
            var poleLen = (int)Math.Round(lengthCm * 0.7, MidpointRounding.AwayFromZero);
            return poles.OrderBy(p => Math.Abs(p.ДлинаСм - poleLen)).FirstOrDefault();
        }

        private static T? PickBySize<T>(IEnumerable<T> items, string size) where T : class
        {
            var prop = typeof(T).GetProperty("Размер");
            return items.FirstOrDefault(x =>
                string.Equals(Convert.ToString(prop?.GetValue(x), CultureInfo.InvariantCulture), size, StringComparison.OrdinalIgnoreCase));
        }
    }
}
