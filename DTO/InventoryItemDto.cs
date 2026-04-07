namespace Prokat.API.DTO
{
    /// <summary>Ответ API для фронта (латиница в JSON).</summary>
    public class InventoryItemDto
    {
        public int Id { get; set; }
        public string? Skis { get; set; }
        public string? Poles { get; set; }
        public string? Snowboard { get; set; }
        public string? Boots { get; set; }
        public string? Helmet { get; set; }
        public string? Goggles { get; set; }
        public bool Recommended { get; set; }
    }
}
