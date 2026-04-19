using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prokat.API.Data;
using Prokat.API.DTO;
using Prokat.API.Services;

namespace Prokat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ISiteSettingsService _settings;
        private readonly ApplicationDbContext _db;

        public SettingsController(ISiteSettingsService settings, ApplicationDbContext db)
        {
            _settings = settings;
            _db = db;
        }

        [HttpGet("vat")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVat(CancellationToken ct)
        {
            var v = await _settings.GetVatRateAsync(ct);
            return Ok(new VatSettingsDto { VatRate = v });
        }

        [HttpPut("vat")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutVat([FromBody] VatUpdateRequest body, CancellationToken ct)
        {
            try
            {
                if (body.VatRate != 0.18m && body.VatRate != 0.20m)
                    return BadRequest(new { message = "Допустимые ставки НДС: 0.18 или 0.20." });

                await _settings.SetVatRateAsync(body.VatRate, ct);
                return Ok(new VatSettingsDto { VatRate = body.VatRate });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("tariffs")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTariffs(CancellationToken ct)
        {
            var rows = await _db.PriceTariffs.AsNoTracking()
                .OrderBy(x => x.Время_аренды)
                .Select(x => new
                {
                    durationHours = x.Время_аренды,
                    rentalWeekday = x.Прокат_будни,
                    rentalWeekend = x.Прокат_выходные_и_праздничные_дни,
                    skiPassWeekday = x.Скипасс_будни,
                    skiPassWeekend = x.Скипасс_выходные_и_праздиничные_дни
                })
                .ToListAsync(ct);
            return Ok(rows);
        }
    }
}
