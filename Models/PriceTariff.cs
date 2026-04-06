namespace Prokat.API.Models
{
    /// <summary>Тарифы (таблица Цены_на_услуги).</summary>
    public class PriceTariff
    {
        public int Время_аренды { get; set; }
        public int? Прокат_будни { get; set; }
        public int? Прокат_выходные_и_праздничные_дни { get; set; }
        public int? Скипасс_будни { get; set; }
        public int? Скипасс_выходные_и_праздиничные_дни { get; set; }
        public int? Абонемент { get; set; }
        public int? Штраф { get; set; }
        public int? ID_Администратора { get; set; }
    }
}
