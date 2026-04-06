namespace Prokat.API.Models
{
    /// <summary>Одна строка настроек (ID = 1).</summary>
    public class SiteSettings
    {
        public int ID { get; set; }
        /// <summary>Ставка НДС, например 0.18 для 18%.</summary>
        public decimal СтавкаНДС { get; set; } = 0.18m;
    }
}
