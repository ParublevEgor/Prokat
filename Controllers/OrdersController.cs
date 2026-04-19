using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prokat.API.Data;
using Prokat.API.Services;

namespace Prokat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderPricingService _pricing;

        public OrdersController(ApplicationDbContext context, IOrderPricingService pricing)
        {
            _context = context;
            _pricing = pricing;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(CancellationToken ct)
        {
            var list = await _context.Orders.AsNoTracking().OrderByDescending(o => o.ID_Заказа).ToListAsync(ct);
            return Ok(list);
        }

        [HttpGet("tariffs/public")]
        public async Task<IActionResult> GetPublicTariffs(CancellationToken ct)
        {
            var rows = await _context.PriceTariffs.AsNoTracking()
                .OrderBy(t => t.Время_аренды)
                .Select(t => new
                {
                    DurationHours = t.Время_аренды,
                    DurationText = t.Время_аренды >= 12 ? "Весь день" : $"{t.Время_аренды} ч",
                    WeekdayPrice = t.Прокат_будни,
                    WeekendPrice = t.Прокат_выходные_и_праздничные_дни
                })
                .ToListAsync(ct);

            return Ok(rows);
        }

        /// <summary>Пересчёт стоимости в коде (идемпотентно по тарифу и датам аренды).</summary>
        [HttpPost("calculate/{orderId}")]
        public async Task<IActionResult> CalculateOrder(int orderId, [FromQuery] decimal vat = 0.18m, CancellationToken ct = default)
        {
            try
            {
                var (базовая, прокат, скипасс, сНдс) = await _pricing.ApplyPricingAsync(orderId, vat, includeSkiPass: false, ct: ct);
                return Ok(new { orderId, baseAmount = базовая, rentalAmount = прокат, skiPassAmount = скипасс, totalWithVat = сНдс });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
