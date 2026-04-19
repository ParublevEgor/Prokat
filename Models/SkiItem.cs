namespace Prokat.API.Models
{
    public class SkiItem
    {
        public int ID_Лыжи { get; set; }
        public string Название { get; set; } = "";
        public string Тип { get; set; } = "";
        public int РостовкаСм { get; set; }
        public string? Уровень { get; set; }
        public string? Примечание { get; set; }
    }
}
