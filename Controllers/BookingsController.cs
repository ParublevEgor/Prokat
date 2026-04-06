using Microsoft.AspNetCore.Mvc;
using Prokat.API.DTO;
using Prokat.API.Services;

namespace Prokat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            try
            {
                var result = await _bookings.CreateBookingAsync(request, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
