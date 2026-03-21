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
        public DbSet<InventoryDto> InventoryDto { get; set; }
        public DbSet<OrderResultDto> OrderResult { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ключ для Client
            modelBuilder.Entity<Client>()
                .HasKey(c => c.ID_Клиента);

            // ключ для Order
            modelBuilder.Entity<Order>()
                .HasKey(o => o.ID_Заказа);

            // ключ для Inventory
            modelBuilder.Entity<Inventory>()
                .HasKey(i => i.ID_Инвентаря);

            // DTO без ключа
            modelBuilder.Entity<InventoryDto>()
                .HasNoKey();

            modelBuilder.Entity<Client>()
                .ToTable("Клиент")
                .HasKey(c => c.ID_Клиента);

            modelBuilder.Entity<Order>()
                .ToTable("Счет_оплаты")
                .HasKey(o => o.ID_Заказа);

            modelBuilder.Entity<Inventory>()
                .ToTable("Инвентарь")
                .HasKey(i => i.ID_Инвентаря);

            modelBuilder.Entity<InventoryDto>()
                .HasNoKey();

            modelBuilder.Entity<OrderResultDto>().HasNoKey();

            modelBuilder.Entity<OrderResultDto>()
                .Property(p => p.ИтоговаяСумма)
                .HasPrecision(10, 2);
        }
    }
}