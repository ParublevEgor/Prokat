using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prokat.API.DTO;
using Prokat.API.Services;

namespace Prokat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ISiteSettingsService _settings;

        public SettingsController(ISiteSettingsService settings)
        {
            _settings = settings;
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
                await _settings.SetVatRateAsync(body.VatRate, ct);
                return Ok(new VatSettingsDto { VatRate = body.VatRate });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
