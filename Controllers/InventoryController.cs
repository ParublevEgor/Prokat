using Microsoft.AspNetCore.Mvc;
using Prokat.API.Services;

namespace Prokat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryAvailabilityService _inventory;

        public InventoryController(IInventoryAvailabilityService inventory)
        {
            _inventory = inventory;
        }

        [HttpGet("free")]
        public async Task<IActionResult> GetFreeInventory(
            [FromQuery] string? type,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            CancellationToken ct)
        {
            var result = await _inventory.GetFreeAsync(type, start, end, ct);
            return Ok(result);
        }
    }
}
