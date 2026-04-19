using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            catch (DbUpdateException ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message = $"Ошибка сохранения брони: {message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Внутренняя ошибка бронирования: {ex.Message}" });
            }
        }

        [HttpPost("quote")]
        public async Task<IActionResult> Quote([FromBody] BookingQuoteRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _bookings.QuoteAsync(request, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("skipass")]
        public async Task<IActionResult> BuySkipPass([FromBody] SkipPassPurchaseRequest request, CancellationToken ct)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? userId = int.TryParse(idStr, out var uid) ? uid : null;
            try
            {
                var result = await _bookings.CreateSkipPassPurchaseAsync(request, userId, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message = $"Ошибка сохранения покупки ски-пасса: {message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Внутренняя ошибка покупки ски-пасса: {ex.Message}" });
            }
        }
    }
}
