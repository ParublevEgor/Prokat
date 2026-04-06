using Microsoft.EntityFrameworkCore;
using Prokat.API.Data;
using Prokat.API.Models;

namespace Prokat.API.Services
{
    public interface ISiteSettingsService
    {
        Task<decimal> GetVatRateAsync(CancellationToken ct = default);
        Task SetVatRateAsync(decimal vatRate, CancellationToken ct = default);
    }

    public class SiteSettingsService : ISiteSettingsService
    {
        private readonly ApplicationDbContext _db;

        public SiteSettingsService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<decimal> GetVatRateAsync(CancellationToken ct = default)
        {
            var row = await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync(ct);
            return row?.СтавкаНДС ?? 0.18m;
        }

        public async Task SetVatRateAsync(decimal vatRate, CancellationToken ct = default)
        {
            if (vatRate < 0 || vatRate > 1)
                throw new InvalidOperationException("Ставка НДС должна быть от 0 до 1 (например 0.18 для 18%).");

            var row = await _db.SiteSettings.FirstOrDefaultAsync(ct);
            if (row is null)
            {
                _db.SiteSettings.Add(new SiteSettings { ID = 1, СтавкаНДС = vatRate });
            }
            else
            {
                row.СтавкаНДС = vatRate;
            }

            await _db.SaveChangesAsync(ct);
        }
    }
}
