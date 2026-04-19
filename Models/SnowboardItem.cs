namespace Prokat.API.Models
{
    public class SnowboardItem
    {
        public int ID_Сноуборд { get; set; }
        public string Название { get; set; } = "";
        public string Тип { get; set; } = "";
        public int РостовкаСм { get; set; }
        public string? Жесткость { get; set; }
        public string? Примечание { get; set; }
    }
}
