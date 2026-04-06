using System.ComponentModel.DataAnnotations;

namespace Prokat.API.DTO
{
    public class BookingCreateRequest
    {
        [Required]
        public string LastName { get; set; } = "Иванов";

        [Required]
        public string FirstName { get; set; } = "Иван";

        public int? Age { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }
        public int? Deposit { get; set; }

        /// <summary>Лыжи или Сноуборд</summary>
        [Required]
        public string EquipmentType { get; set; } = "Лыжи";

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int? InventoryId { get; set; }

        public decimal VatRate { get; set; } = 0.18m;
    }
}
