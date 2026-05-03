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



        /// <summary>Заказы клиента: аренды и отдельные покупки ски-пасса.</summary>

        [HttpGet("my")]

        public async Task<IActionResult> MyOrders(CancellationToken ct)

        {

            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(idStr, out var userId))

                return Unauthorized();



            var clientId = await _db.AppUsers.AsNoTracking()

                .Where(u => u.ID_Учетной_записи == userId)

                .Select(u => u.ID_Клиента)

                .FirstOrDefaultAsync(ct);



            if (clientId is null)

                return Ok(Array.Empty<MyOrderHistoryItemDto>());



            var cid = clientId.Value;



            var rentalRaw = await _db.RentalBookings.AsNoTracking()

                .Include(r => r.Order)

                .Include(r => r.Inventory!).ThenInclude(i => i!.Лыжи)

                .Include(r => r.Inventory!).ThenInclude(i => i!.Сноуборд)

                .Include(r => r.Inventory!).ThenInclude(i => i!.Ботинки)

                .Include(r => r.Inventory!).ThenInclude(i => i!.Палки)

                .Include(r => r.Inventory!).ThenInclude(i => i!.Шлем)

                .Include(r => r.Inventory!).ThenInclude(i => i!.Очки)

                .Where(r => r.Order != null && r.Order.ID_Клиента == cid)

                .OrderByDescending(r => r.ДатаНачала)

                .ToListAsync(ct);



            var rentalRows = rentalRaw.Select(r => new MyOrderHistoryItemDto

            {

                OrderId = r.ID_Заказа,

                Kind = "Аренда",

                RentalId = r.ID_Аренды,

                Start = r.ДатаНачала,

                End = r.ДатаОкончания,

                Status = r.Статус,

                Details = BuildInventorySummary(r.Inventory!) ?? "",

                TotalWithVat = r.Order!.Сумма_оплаты,

            }).ToList();



            var skipRows = await _db.Orders.AsNoTracking()

                .Where(o => o.ID_Клиента == cid)

                .Where(o => !_db.RentalBookings.Any(rb => rb.ID_Заказа == o.ID_Заказа))

                .OrderByDescending(o => o.ID_Заказа)

                .Select(o => new MyOrderHistoryItemDto

                {

                    OrderId = o.ID_Заказа,

                    Kind = "Ски-пасс",

                    RentalId = null,

                    Start = null,

                    End = null,

                    Status = "Оформлен",

                    Details = "Покупка ски-пасса",

                    TotalWithVat = o.Сумма_оплаты,

                })

                .ToListAsync(ct);



            var merged = rentalRows.Concat(skipRows)

                .OrderByDescending(x => x.OrderId)

                .ToList();



            return Ok(merged);

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


