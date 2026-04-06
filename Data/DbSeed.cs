using Microsoft.EntityFrameworkCore;
using Prokat.API.Models;

namespace Prokat.API.Data
{
    public static class DbSeed
    {
        public static void EnsureDefaults(ApplicationDbContext db)
        {
            if (!db.SiteSettings.AsNoTracking().Any())
            {
                db.SiteSettings.Add(new SiteSettings { СтавкаНДС = 0.18m });
                db.SaveChanges();
            }

            if (!db.AppUsers.AsNoTracking().Any(u => u.Логин == "admin"))
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
        }
    }
}
