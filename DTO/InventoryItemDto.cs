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

        /// <summary>Заголовок карточки без акцента на бренд (рус.).</summary>
        public string? CardTitle { get; set; }

        /// <summary>Вторичная строка: ботинки, шлем, маска.</summary>
        public string? CardSubtitle { get; set; }

        /// <summary>S / M / L, если удалось извлечь.</summary>
        public string? SizeClass { get; set; }

        /// <summary>Длина лыж или доски, см.</summary>
        public int? LengthCmHint { get; set; }

        /// <summary>Размер ботинок EU.</summary>
        public int? BootSizeEuHint { get; set; }

        /// <summary>Исходное название модели для блока «Подробнее».</summary>
        public string? ModelReference { get; set; }
    }
}
