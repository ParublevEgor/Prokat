namespace Prokat.API.DTO
{
    public class BookingQuoteRequest
    {
        public DateTime RentalDate { get; set; }
        public string DurationKey { get; set; } = "2";
        public bool IncludeSkiPass { get; set; }
    }

    public class BookingQuoteDto
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal RentalAmount { get; set; }
        public decimal SkiPassAmount { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal VatRate { get; set; }
        public decimal TotalWithVat { get; set; }
    }

    public class SkipPassPurchaseRequest
    {
        /// <summary>weekday | weekend — без выбора календарной даты.</summary>
        public string DayKind { get; set; } = "weekday";
        /// <summary>time | lifts</summary>
        public string Mode { get; set; } = "time";
        /// <summary>Для mode=time: 2, 3, 4, day</summary>
        public string? TimeSlot { get; set; }
        /// <summary>Для mode=lifts: 15, 30, 50, 100</summary>
        public int? LiftCount { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public int? Age { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }
        public int? Deposit { get; set; }
    }

    public class SkipPassPurchaseResultDto
    {
        public int OrderId { get; set; }
        public int ClientId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal SkiPassAmount { get; set; }
        public decimal TotalWithVat { get; set; }
    }
}
