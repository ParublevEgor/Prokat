namespace Prokat.API.Models
{
    public class Inventory
    {
        public int ID_Инвентаря { get; set; }
        public int? Скипасс { get; set; }
        public string? Лыжи { get; set; }
        public string? Палки { get; set; }
        public string? Сноуборд { get; set; }
        public string? Ботинки { get; set; }
        public string? Шлем { get; set; }
        public string? Маска { get; set; }
        public string? Одежда { get; set; }
        public int? ID_Техника_консультанта { get; set; }
    }
}