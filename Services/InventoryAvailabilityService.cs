using Microsoft.EntityFrameworkCore;
using Prokat.API.Data;
using Prokat.API.DTO;

namespace Prokat.API.Services
{
    public interface IInventoryAvailabilityService
    {
        Task<IReadOnlyList<InventoryItemDto>> GetFreeAsync(string? типИнвентаря, DateTime начало, DateTime конец, CancellationToken ct = default);
        Task<IReadOnlyList<InventoryItemDto>> GetRecommendedFreeAsync(string? типИнвентаря, DateTime начало, DateTime конец, int? shoeSize, int? height, CancellationToken ct = default);
    }

    public class InventoryAvailabilityService : IInventoryAvailabilityService
    {
        private readonly ApplicationDbContext _db;

        public InventoryAvailabilityService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<InventoryItemDto>> GetFreeAsync(string? типИнвентаря, DateTime начало, DateTime конец, CancellationToken ct = default)
            => await GetRecommendedFreeAsync(типИнвентаря, начало, конец, null, null, ct);

        public async Task<IReadOnlyList<InventoryItemDto>> GetRecommendedFreeAsync(string? типИнвентаря, DateTime начало, DateTime конец, int? shoeSize, int? height, CancellationToken ct = default)
        {
            if (конец <= начало)
                return Array.Empty<InventoryItemDto>();

            var тип = (типИнвентаря ?? "").Trim();
            var busyIds = await _db.RentalBookings
                .Where(r => r.Статус != "Отмена")
                .Where(r => r.ДатаНачала < конец && начало < r.ДатаОкончания)
                .Select(r => r.ID_Инвентаря)
                .Distinct()
                .ToListAsync(ct);

            var query = _db.Inventory.AsQueryable();
            if (string.Equals(тип, "Лыжи", StringComparison.OrdinalIgnoreCase))
                query = query.Where(i => i.Лыжи != null && i.Лыжи != "");
            else if (string.Equals(тип, "Сноуборд", StringComparison.OrdinalIgnoreCase))
                query = query.Where(i => i.Сноуборд != null && i.Сноуборд != "");

            var list = await query
                .Where(i => !busyIds.Contains(i.ID_Инвентаря))
                .OrderBy(i => i.ID_Инвентаря)
                .Select(i => new InventoryItemDto
                {
                    Id = i.ID_Инвентаря,
                    Skis = i.Лыжи,
                    Poles = i.Палки,
                    Snowboard = i.Сноуборд,
                    Boots = i.Ботинки,
                    Helmet = i.Шлем,
                    Goggles = i.Маска,
                    Recommended = IsRecommended(i.Ботинки, i.Лыжи, i.Сноуборд, shoeSize, height),
                })
                .ToListAsync(ct);

            return list;
        }

        private static bool IsRecommended(string? boots, string? skis, string? snowboard, int? shoeSize, int? height)
        {
            var byShoe = shoeSize is null || string.IsNullOrEmpty(boots) || boots.Contains(shoeSize.Value.ToString(), StringComparison.OrdinalIgnoreCase);
            var byHeight = height is null
                || (height < 165 && ((skis ?? "").Contains("S") || (snowboard ?? "").Contains("S")))
                || (height >= 165 && height < 180 && ((skis ?? "").Contains("M") || (snowboard ?? "").Contains("M")))
                || (height >= 180 && ((skis ?? "").Contains("L") || (snowboard ?? "").Contains("L")));
            return byShoe && byHeight;
        }
    }
}
