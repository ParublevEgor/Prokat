namespace Prokat.API.Models
{
    public class Client
    {
        public int ID_Клиента { get; set; }
        public string? Фамилия { get; set; }
        public string? Имя { get; set; }
        public int? Возраст { get; set; }
        public int? Рост { get; set; }
        public int? Вес { get; set; }
        public int? Залог { get; set; }
        public int? ID_Заказа { get; set; }
    }
}