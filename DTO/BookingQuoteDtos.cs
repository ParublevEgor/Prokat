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
        public DateTime RentalDate { get; set; }
        public string DurationKey { get; set; } = "2";
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
