namespace Prokat.API.Models
{
    public class BootsItem
    {
        public int ID_Ботинки { get; set; }
        public string Название { get; set; } = "";
        public string Тип { get; set; } = "";
        public int РазмерEU { get; set; }
        public string? Примечание { get; set; }
    }
}
