using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prokat.API.Data;
using Prokat.API.DTO;
using Prokat.API.Models;
using System.Security.Claims;

namespace Prokat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class RentalsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public RentalsController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>История аренд текущего пользователя (по клиенту из учётной записи).</summary>
        [HttpGet("my")]
        public async Task<IActionResult> MyHistory(CancellationToken ct)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var userId))
                return Unauthorized();

            var clientId = await _db.AppUsers.AsNoTracking()
                .Where(u => u.ID_Учетной_записи == userId)
                .Select(u => u.ID_Клиента)
                .FirstOrDefaultAsync(ct);

            if (clientId is null)
                return Ok(Array.Empty<RentalHistoryItemDto>());

            var rows = await (
                from r in _db.RentalBookings.AsNoTracking()
                join o in _db.Orders on r.ID_Заказа equals o.ID_Заказа
                join i in _db.Inventory
                    .Include(x => x.Лыжи)
                    .Include(x => x.Сноуборд)
                    .Include(x => x.Ботинки)
                    .Include(x => x.Палки)
                    .Include(x => x.Шлем)
                    .Include(x => x.Очки)
                    on r.ID_Инвентаря equals i.ID_Инвентаря
                where o.ID_Клиента == clientId
                orderby r.ДатаНачала descending
                select new RentalHistoryItemDto
                {
                    RentalId = r.ID_Аренды,
                    OrderId = r.ID_Заказа,
                    Start = r.ДатаНачала,
                    End = r.ДатаОкончания,
                    Status = r.Статус,
                    TotalWithVat = o.Сумма_оплаты,
                    InventorySummary = BuildInventorySummary(i),
                }).ToListAsync(ct);

            return Ok(rows);
        }

        private static string? BuildInventorySummary(Inventory i)
        {
            var parts = new[]
            {
                i.Лыжи == null ? null : $"{i.Лыжи.Название} {i.Лыжи.РостовкаСм} см",
                i.Сноуборд == null ? null : $"{i.Сноуборд.Название} {i.Сноуборд.РостовкаСм} см",
                i.Ботинки == null ? null : $"Ботинки EU {i.Ботинки.РазмерEU}",
                i.Палки == null ? null : $"Палки {i.Палки.ДлинаСм} см"
            }
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Take(2);
            var s = string.Join(", ", parts);
            return string.IsNullOrEmpty(s) ? $"ID {i.ID_Инвентаря}" : s;
        }
    }
}
