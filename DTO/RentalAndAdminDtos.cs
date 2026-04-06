namespace Prokat.API.DTO
{
    public class RentalHistoryItemDto
    {
        public int RentalId { get; set; }
        public int OrderId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Status { get; set; } = "";
        public decimal? TotalWithVat { get; set; }
        public string? InventorySummary { get; set; }
    }

    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Login { get; set; } = "";
        public string Role { get; set; } = "";
        public int? ClientId { get; set; }
    }

    public class AdminRentalDto
    {
        public int RentalId { get; set; }
        public int OrderId { get; set; }
        public int InventoryId { get; set; }
        public string? ClientName { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Status { get; set; } = "";
        public decimal? TotalWithVat { get; set; }
        public string? InventorySummary { get; set; }
    }

    public class AdminStatsDto
    {
        public int RegisteredUsers { get; set; }
        public int Administrators { get; set; }
        public int TotalOrders { get; set; }
        public int ActiveRentals { get; set; }
    }

    public class MeResponseDto
    {
        public int UserId { get; set; }
        public string Login { get; set; } = "";
        public string Role { get; set; } = "";
        public int? ClientId { get; set; }
        public string Auth { get; set; } = "Bearer";
        public string TokenType { get; set; } = "JWT";
    }
}
