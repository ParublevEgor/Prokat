using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prokat.API.Data;
using Prokat.API.DTO;
using System.Threading.Tasks;

namespace Prokat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Расчет стоимости
        [HttpPost("calculate/{orderId}")]
        public async Task<IActionResult> CalculateOrder(int orderId)
        {
            var result = await _context.Set<OrderResultDto>()
                .FromSqlRaw("EXEC sp_РассчитатьСтоимостьЗаказа @ID_Заказа = {0}", orderId)
                .ToListAsync();

            return Ok(result);
        }
    }
}