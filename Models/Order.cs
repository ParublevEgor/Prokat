namespace Prokat.API.Models
{
    public class Order
    {
        public int ID_Заказа { get; set; }
        public int? ID_Клиента { get; set; }
        /// <summary>Итоговая сумма с НДС (руб.).</summary>
        public decimal? Сумма_оплаты { get; set; }
        /// <summary>Сумма без НДС до применения ставки (руб.).</summary>
        public decimal? БазоваяСумма { get; set; }
        public int? ID_Администратора { get; set; }
        public int? ID_Техника_консультанта { get; set; }

        public ICollection<RentalBooking> Rentals { get; set; } = new List<RentalBooking>();
    }
}