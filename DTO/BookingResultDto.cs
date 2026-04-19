namespace Prokat.API.DTO
{
    public class BookingResultDto
    {
        public int OrderId { get; set; }
        public int ClientId { get; set; }
        public int RentalId { get; set; }
        public int InventoryId { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal RentalAmount { get; set; }
        public decimal SkiPassAmount { get; set; }
        public decimal TotalWithVat { get; set; }
    }
}
