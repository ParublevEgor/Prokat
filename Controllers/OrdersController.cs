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

        /// <summary>Пересчёт стоимости в коде (идемпотентно по тарифу и датам аренды).</summary>
        [HttpPost("calculate/{orderId}")]
        public async Task<IActionResult> CalculateOrder(int orderId, [FromQuery] decimal vat = 0.18m, CancellationToken ct = default)
        {
            try
            {
                var (базовая, сНдс) = await _pricing.ApplyPricingAsync(orderId, vat, ct);
                return Ok(new { orderId, baseAmount = базовая, totalWithVat = сНдс });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
