namespace Prokat.API.Models
{
    public class PolesItem
    {
        public int ID_Палки { get; set; }
        public string Название { get; set; } = "";
        public string Тип { get; set; } = "";
        public int ДлинаСм { get; set; }
        public string? Примечание { get; set; }
    }
}
