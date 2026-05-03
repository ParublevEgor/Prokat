namespace Prokat.API.Models
{
    public class RentalBooking
    {
        public int ID_Аренды { get; set; }
        public int ID_Заказа { get; set; }
        public int ID_Инвентаря { get; set; }
        public DateTime ДатаНачала { get; set; }
        public DateTime ДатаОкончания { get; set; }
        public string Статус { get; set; } = "Бронь";

        public Order? Order { get; set; }
        public Inventory? Inventory { get; set; }
    }
}
