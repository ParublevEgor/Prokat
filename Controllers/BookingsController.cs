using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prokat.API.DTO;
using Prokat.API.Services;
using System.Security.Claims;

namespace Prokat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class BookingsController : ControllerBase
    {
        private readonly IRentalBookingService _bookings;

        public BookingsController(IRentalBookingService bookings)
        {
            _bookings = bookings;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BookingCreateRequest request, CancellationToken ct)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? userId = int.TryParse(idStr, out var uid) ? uid : null;

            try
            {
                var result = await _bookings.CreateBookingAsync(request, userId, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
