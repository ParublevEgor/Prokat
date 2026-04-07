using Microsoft.EntityFrameworkCore;
using Prokat.API.Models;
using System.Data;

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
            // Добавляем тестовый набор только если ещё нет записей с заполненным шлемом
            var hasFullData = db.Inventory.AsNoTracking()
                .Any(i => i.Шлем != null && i.Шлем != "");
            if (hasFullData) return;

            var rows = new[]
            {
                // Лыжи — маленький размер (рост ~150–165)
                new Inventory{ Лыжи="Лыжи S 150см", Палки="Палки S 105см", Ботинки="Ботинки 38", Шлем="Шлем S", Маска="Маска S" },
                new Inventory{ Лыжи="Лыжи S 155см", Палки="Палки S 105см", Ботинки="Ботинки 37", Шлем="Шлем S", Маска="Маска S" },
                new Inventory{ Лыжи="Лыжи S 155см", Палки="Палки S 110см", Ботинки="Ботинки 39", Шлем="Шлем S", Маска="Маска S" },

                // Лыжи — средний размер (рост ~165–180)
                new Inventory{ Лыжи="Лыжи M 165см", Палки="Палки M 115см", Ботинки="Ботинки 40", Шлем="Шлем M", Маска="Маска M" },
                new Inventory{ Лыжи="Лыжи M 170см", Палки="Палки M 115см", Ботинки="Ботинки 41", Шлем="Шлем M", Маска="Маска M" },
                new Inventory{ Лыжи="Лыжи M 170см", Палки="Палки M 120см", Ботинки="Ботинки 42", Шлем="Шлем M", Маска="Маска M" },
                new Inventory{ Лыжи="Лыжи M 175см", Палки="Палки M 120см", Ботинки="Ботинки 41", Шлем="Шлем M", Маска="Маска M" },

                // Лыжи — большой размер (рост 180+)
                new Inventory{ Лыжи="Лыжи L 180см", Палки="Палки L 125см", Ботинки="Ботинки 43", Шлем="Шлем L", Маска="Маска L" },
                new Inventory{ Лыжи="Лыжи L 185см", Палки="Палки L 130см", Ботинки="Ботинки 44", Шлем="Шлем L", Маска="Маска L" },
                new Inventory{ Лыжи="Лыжи L 185см", Палки="Палки L 130см", Ботинки="Ботинки 45", Шлем="Шлем L", Маска="Маска L" },

                // Сноуборд — маленький
                new Inventory{ Сноуборд="Сноуборд S 140см", Ботинки="Ботинки 37", Шлем="Шлем S", Маска="Маска S" },
                new Inventory{ Сноуборд="Сноуборд S 145см", Ботинки="Ботинки 38", Шлем="Шлем S", Маска="Маска S" },
                new Inventory{ Сноуборд="Сноуборд S 148см", Ботинки="Ботинки 39", Шлем="Шлем S", Маска="Маска S" },

                // Сноуборд — средний
                new Inventory{ Сноуборд="Сноуборд M 152см", Ботинки="Ботинки 40", Шлем="Шлем M", Маска="Маска M" },
                new Inventory{ Сноуборд="Сноуборд M 155см", Ботинки="Ботинки 41", Шлем="Шлем M", Маска="Маска M" },
                new Inventory{ Сноуборд="Сноуборд M 158см", Ботинки="Ботинки 42", Шлем="Шлем M", Маска="Маска M" },
                new Inventory{ Сноуборд="Сноуборд M 160см", Ботинки="Ботинки 41", Шлем="Шлем M", Маска="Маска M" },

                // Сноуборд — большой
                new Inventory{ Сноуборд="Сноуборд L 162см", Ботинки="Ботинки 43", Шлем="Шлем L", Маска="Маска L" },
                new Inventory{ Сноуборд="Сноуборд L 165см", Ботинки="Ботинки 44", Шлем="Шлем L", Маска="Маска L" },
                new Inventory{ Сноуборд="Сноуборд L 168см", Ботинки="Ботинки 45", Шлем="Шлем L", Маска="Маска L" },
            };

            db.Inventory.AddRange(rows);
            db.SaveChanges();
        }
    }
}
