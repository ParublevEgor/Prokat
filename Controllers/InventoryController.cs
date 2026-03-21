using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prokat.API.Data;
using Prokat.API.DTO;
using System.Threading.Tasks;

namespace Prokat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Вызов хранимой процедуры
        [HttpGet("free")]
        public async Task<IActionResult> GetFreeInventory(string? type)
        {
            var result = await _context.Set<InventoryDto>()
                .FromSqlRaw("EXEC sp_НайтиСвободныйИнвентарь @ТипИнвентаря = {0}", type ?? "")
                .ToListAsync();

            return Ok(result);
        }
    }
}