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
        public DbSet<Order> Orders { get; set; }
        public DbSet<RentalBooking> RentalBookings { get; set; }
        public DbSet<PriceTariff> PriceTariffs { get; set; }

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

            modelBuilder.Entity<PriceTariff>()
                .ToTable("Цены_на_услуги")
                .HasKey(p => p.Время_аренды);

            modelBuilder.Entity<PriceTariff>()
                .Property(p => p.Время_аренды)
                .ValueGeneratedNever();

            modelBuilder.Entity<RentalBooking>()
                .ToTable("Аренда_бронирование")
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
