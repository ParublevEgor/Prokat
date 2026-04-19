using Microsoft.EntityFrameworkCore;
using Prokat.API.Models;
using Prokat.API.DTO;

namespace Prokat.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<SkiItem> Skis { get; set; }
        public DbSet<SnowboardItem> Snowboards { get; set; }
        public DbSet<BootsItem> Boots { get; set; }
        public DbSet<PolesItem> Poles { get; set; }
        public DbSet<HelmetItem> Helmets { get; set; }
        public DbSet<GogglesItem> Goggles { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<RentalBooking> RentalBookings { get; set; }
        public DbSet<PriceTariff> PriceTariffs { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<SiteSettings> SiteSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>()
                .ToTable("Клиент")
                .HasKey(c => c.ID_Клиента);

            modelBuilder.Entity<Order>()
                .ToTable("Счет_оплаты")
                .HasKey(o => o.ID_Заказа);

            modelBuilder.Entity<Inventory>()
                .ToTable("Инвентарь")
                .HasKey(i => i.ID_Инвентаря);

            modelBuilder.Entity<SkiItem>()
                .ToTable("Лыжи")
                .HasKey(i => i.ID_Лыжи);

            modelBuilder.Entity<SnowboardItem>()
                .ToTable("Сноуборды")
                .HasKey(i => i.ID_Сноуборд);

            modelBuilder.Entity<BootsItem>()
                .ToTable("Ботинки")
                .HasKey(i => i.ID_Ботинки);

            modelBuilder.Entity<PolesItem>()
                .ToTable("Палки")
                .HasKey(i => i.ID_Палки);

            modelBuilder.Entity<HelmetItem>()
                .ToTable("Шлемы")
                .HasKey(i => i.ID_Шлем);

            modelBuilder.Entity<GogglesItem>()
                .ToTable("Очки")
                .HasKey(i => i.ID_Очки);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Лыжи)
                .WithMany()
                .HasForeignKey(i => i.ID_Лыжи)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Сноуборд)
                .WithMany()
                .HasForeignKey(i => i.ID_Сноуборд)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Ботинки)
                .WithMany()
                .HasForeignKey(i => i.ID_Ботинки)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Палки)
                .WithMany()
                .HasForeignKey(i => i.ID_Палки)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Шлем)
                .WithMany()
                .HasForeignKey(i => i.ID_Шлем)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Очки)
                .WithMany()
                .HasForeignKey(i => i.ID_Очки)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PriceTariff>()
                .ToTable("Цены_на_услуги")
                .HasKey(p => p.Время_аренды);

            modelBuilder.Entity<PriceTariff>()
                .Property(p => p.Время_аренды)
                .ValueGeneratedNever();

            modelBuilder.Entity<RentalBooking>()
                // Для SQL Server с триггерами: отключаем быстрый OUTPUT без INTO
                .ToTable("Аренда_бронирование", t => t.HasTrigger("TRG_Аренда_Проверки"))
                .HasKey(r => r.ID_Аренды);

            modelBuilder.Entity<RentalBooking>()
                .Property(r => r.Статус)
                .HasMaxLength(20);

            modelBuilder.Entity<RentalBooking>()
                .HasOne(r => r.Order)
                .WithMany(o => o.Rentals)
                .HasForeignKey(r => r.ID_Заказа)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RentalBooking>()
                .HasOne(r => r.Inventory)
                .WithMany(i => i.Rentals)
                .HasForeignKey(r => r.ID_Инвентаря)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppUser>()
                .ToTable("Учетная_запись")
                .HasKey(u => u.ID_Учетной_записи);

            modelBuilder.Entity<AppUser>()
                .Property(u => u.Логин)
                .HasMaxLength(64)
                .IsRequired();

            modelBuilder.Entity<AppUser>()
                .Property(u => u.ПарольХеш)
                .HasMaxLength(256)
                .IsRequired();

            modelBuilder.Entity<AppUser>()
                .Property(u => u.Роль)
                .HasMaxLength(16)
                .IsRequired();

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Логин)
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Client)
                .WithMany()
                .HasForeignKey(u => u.ID_Клиента)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SiteSettings>()
                .ToTable("Настройки_проката")
                .HasKey(s => s.ID);

            modelBuilder.Entity<SiteSettings>()
                .Property(s => s.СтавкаНДС)
                .HasPrecision(5, 4);

            modelBuilder.Entity<Order>()
                .Property(o => o.Сумма_оплаты)
                .HasPrecision(12, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.БазоваяСумма)
                .HasPrecision(12, 2);

            modelBuilder.Entity<InventoryDto>()
                .HasNoKey()
                .ToTable("InventoryDto", t => t.ExcludeFromMigrations());

            modelBuilder.Entity<OrderResultDto>()
                .HasNoKey()
                .ToTable("OrderResultDto", t => t.ExcludeFromMigrations());

            modelBuilder.Entity<OrderResultDto>()
                .Property(p => p.ИтоговаяСумма)
                .HasPrecision(10, 2);
        }
    }
}
