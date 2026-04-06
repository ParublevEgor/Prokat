namespace Prokat.API.Models
{
    /// <summary>Бронирование / аренда единицы инвентаря.</summary>
    public class RentalBooking
    {
        public int ID_Аренды { get; set; }
        public int ID_Заказа { get; set; }
        public int ID_Инвентаря { get; set; }
        public DateTime ДатаНачала { get; set; }
        public DateTime ДатаОкончания { get; set; }
        /// <summary>Бронь, Выдано, Возвращено, Отмена</summary>
        public string Статус { get; set; } = "Бронь";

        public Order? Order { get; set; }
        public Inventory? Inventory { get; set; }
    }
}
