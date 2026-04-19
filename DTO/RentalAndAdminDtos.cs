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
        public bool CanDelete { get; set; }
        public string? DeleteBlockedReason { get; set; }
    }

    /// <summary>Список аренд для админки: без дублирования с заказом — номер аренды; сумма после описания инвентаря.</summary>
    public class AdminRentalDto
    {
        public int RentalId { get; set; }
        public string? ClientName { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Status { get; set; } = "";
        public string? InventorySummary { get; set; }
        public decimal? Total { get; set; }
    }

    public class AdminUserDetailDto
    {
        public int UserId { get; set; }
        public string Login { get; set; } = "";
        public string Role { get; set; } = "";
        public int? ClientId { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public int? Age { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }
        public int? ShoeSize { get; set; }
        public bool HasProfilePhoto { get; set; }
        public int? Deposit { get; set; }
    }

    public class AdminStatsDto
    {
        public int RegisteredUsers { get; set; }
        public int Administrators { get; set; }
        public int TotalOrders { get; set; }
        public int ActiveRentals { get; set; }
    }

    public class AdminInventoryStatusDto
    {
        public int InventoryId { get; set; }
        public string Status { get; set; } = "";
        public string Type { get; set; } = "";
        public string? Skis { get; set; }
        public string? Snowboard { get; set; }
        public string? Boots { get; set; }
        public string? Poles { get; set; }
    }

    public class MeResponseDto
    {
        public int UserId { get; set; }
        public string Login { get; set; } = "";
        public string Role { get; set; } = "";
        public int? ClientId { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public int? Age { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }
        public int? ShoeSize { get; set; }
        public string Auth { get; set; } = "Bearer";
        public string TokenType { get; set; } = "JWT";
        public string? ProfilePhotoBase64 { get; set; }
    }
}
