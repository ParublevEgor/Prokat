using System.ComponentModel.DataAnnotations;

namespace Prokat.API.DTO
{
    public class BookingCreateRequest
    {
        /// <summary>Для администратора без привязки к клиенту — ФИО нового клиента.</summary>
        public string? LastName { get; set; }

        public string? FirstName { get; set; }

        public int? Age { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }
        public int? Deposit { get; set; }

        /// <summary>Лыжи или Сноуборд</summary>
        [Required]
        public string EquipmentType { get; set; } = "Лыжи";

        /// <summary>Календарный день аренды (дата без привязки к часовому поясу — используется дата).</summary>
        [Required]
        public DateTime RentalDate { get; set; }

        /// <summary>1, 2, 4, day — длительность от начала смены (9:00).</summary>
        [Required]
        public string DurationKey { get; set; } = "2";

        /// <summary>Добавить ски-пасс к текущему заказу.</summary>
        public bool IncludeSkiPass { get; set; }

        public int? InventoryId { get; set; }
    }
}
