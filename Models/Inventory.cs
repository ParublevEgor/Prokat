namespace Prokat.API.Models
{
    public class Inventory
    {
        public int ID_Инвентаря { get; set; }
        public int? Скипасс { get; set; }
        public int? ID_Техника_консультанта { get; set; }
        public int? ID_Лыжи { get; set; }
        public int? ID_Сноуборд { get; set; }
        public int? ID_Ботинки { get; set; }
        public int? ID_Палки { get; set; }
        public int? ID_Шлем { get; set; }
        public int? ID_Очки { get; set; }

        public SkiItem? Лыжи { get; set; }
        public SnowboardItem? Сноуборд { get; set; }
        public BootsItem? Ботинки { get; set; }
        public PolesItem? Палки { get; set; }
        public HelmetItem? Шлем { get; set; }
        public GogglesItem? Очки { get; set; }

        public ICollection<RentalBooking> Rentals { get; set; } = new List<RentalBooking>();
    }
}