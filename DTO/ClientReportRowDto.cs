namespace Prokat.API.DTO
{
    public class ClientReportRowDto
    {
        public int ClientId { get; set; }
        public string FullName { get; set; } = "";
        public int? Age { get; set; }
        public int? OrderId { get; set; }
        public decimal? Total { get; set; }
        public string DepositStatus { get; set; } = "";
        public int? RentalId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
