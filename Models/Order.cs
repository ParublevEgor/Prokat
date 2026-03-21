namespace Prokat.API.Models
{
    public class Order
    {
        public int ID_Заказа { get; set; }
        public int? ID_Клиента { get; set; }
        public int? Сумма_оплаты { get; set; }
        public int? ID_Администратора { get; set; }
        public int? ID_Техника_консультанта { get; set; }
    }
}