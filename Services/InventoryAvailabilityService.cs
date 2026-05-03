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

            var query = _db.Inventory
                .Include(i => i.Лыжи)
                .Include(i => i.Сноуборд)
                .Include(i => i.Ботинки)
                .Include(i => i.Палки)
                .Include(i => i.Шлем)
                .Include(i => i.Очки)
                .AsQueryable();
            if (string.Equals(тип, "Skis", StringComparison.OrdinalIgnoreCase)
                || string.Equals(тип, "Лыжи", StringComparison.OrdinalIgnoreCase))
                query = query.Where(i => i.ID_Лыжи != null);
            else if (string.Equals(тип, "Snowboard", StringComparison.OrdinalIgnoreCase)
                || string.Equals(тип, "Сноуборд", StringComparison.OrdinalIgnoreCase))
                query = query.Where(i => i.ID_Сноуборд != null);

            var list = await query
                .Where(i => !busyIds.Contains(i.ID_Инвентаря))
                .OrderBy(i => i.ID_Инвентаря)
                .Select(i => new InventoryItemDto
                {
                    Id = i.ID_Инвентаря,
                    Skis = i.Лыжи == null ? null : $"{i.Лыжи.Название} ({i.Лыжи.Тип}, {i.Лыжи.РостовкаСм} см)",
                    Poles = i.Палки == null ? null : $"{i.Палки.Название} ({i.Палки.ДлинаСм} см)",
                    Snowboard = i.Сноуборд == null ? null : $"{i.Сноуборд.Название} ({i.Сноуборд.Тип}, {i.Сноуборд.РостовкаСм} см)",
                    Boots = i.Ботинки == null ? null : $"{i.Ботинки.Название}, EU {i.Ботинки.РазмерEU}",
                    Helmet = i.Шлем == null ? null : $"{i.Шлем.Название} ({i.Шлем.Размер})",
                    Goggles = i.Очки == null ? null : $"{i.Очки.Название} ({i.Очки.Размер})",
                    Recommended = IsRecommended(i, shoeSize, height),
                })
                .ToListAsync(ct);

            foreach (var item in list)
                InventoryItemPresentation.Enrich(item, тип);

            return list;
        }

        private static bool IsRecommended(Models.Inventory i, int? shoeSize, int? height)
        {
            var byShoe = shoeSize is null || i.Ботинки == null || i.Ботинки.РазмерEU == shoeSize.Value;
            var len = i.Лыжи?.РостовкаСм ?? i.Сноуборд?.РостовкаСм;
            var byHeight = height is null
                || (len is null)
                || (height < 165 && len <= 160)
                || (height >= 165 && height < 180 && len > 160 && len <= 175)
                || (height >= 180 && len > 175);
            return byShoe && byHeight;
        }
    }
}
