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
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> Stats(CancellationToken ct)
        {
            var users = await _db.AppUsers.AsNoTracking().CountAsync(u => u.Роль == "User", ct);
            var admins = await _db.AppUsers.AsNoTracking().CountAsync(u => u.Роль == "Admin", ct);
            var orders = await _db.Orders.AsNoTracking().CountAsync(ct);
            var active = await _db.RentalBookings.AsNoTracking()
                .CountAsync(r => r.Статус != "Отмена", ct);

            return Ok(new AdminStatsDto
            {
                RegisteredUsers = users,
                Administrators = admins,
                TotalOrders = orders,
                ActiveRentals = active,
            });
        }

        [HttpGet("users")]
        public async Task<IActionResult> ListUsers(CancellationToken ct)
        {
            var list = await _db.AppUsers.AsNoTracking()
                .OrderBy(u => u.ID_Учетной_записи)
                .Select(u => new AdminUserDto
                {
                    Id = u.ID_Учетной_записи,
                    Login = u.Логин,
                    Role = u.Роль,
                    ClientId = u.ID_Клиента,
                })
                .ToListAsync(ct);
            return Ok(list);
        }

        [HttpDelete("users/{id:int}")]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken ct)
        {
            var meStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(meStr, out var myId))
                return Unauthorized();

            if (id == myId)
                return BadRequest(new { message = "Нельзя удалить свою учётную запись." });

            var target = await _db.AppUsers.FirstOrDefaultAsync(u => u.ID_Учетной_записи == id, ct);
            if (target is null)
                return NotFound(new { message = "Пользователь не найден." });

            if (target.Роль == "Admin")
                return BadRequest(new { message = "Нельзя удалить учётную запись администратора." });

            _db.AppUsers.Remove(target);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpGet("rentals")]
        public async Task<IActionResult> AllRentals(CancellationToken ct)
        {
            var rows = await (
                from r in _db.RentalBookings.AsNoTracking()
                join o in _db.Orders on r.ID_Заказа equals o.ID_Заказа
                join i in _db.Inventory on r.ID_Инвентаря equals i.ID_Инвентаря
                join c in _db.Clients on o.ID_Клиента equals c.ID_Клиента into cg
                from c in cg.DefaultIfEmpty()
                orderby r.ДатаНачала descending
                select new AdminRentalDto
                {
                    RentalId = r.ID_Аренды,
                    OrderId = r.ID_Заказа,
                    InventoryId = r.ID_Инвентаря,
                    ClientName = c != null ? (c.Фамилия + " " + c.Имя).Trim() : null,
                    Start = r.ДатаНачала,
                    End = r.ДатаОкончания,
                    Status = r.Статус,
                    TotalWithVat = o.Сумма_оплаты,
                    InventorySummary = BuildInventorySummary(i),
                }).ToListAsync(ct);

            return Ok(rows);
        }

        [HttpPost("rentals/{id:int}/cancel")]
        public async Task<IActionResult> CancelRental(int id, CancellationToken ct)
        {
            var r = await _db.RentalBookings.FirstOrDefaultAsync(x => x.ID_Аренды == id, ct);
            if (r is null)
                return NotFound(new { message = "Аренда не найдена." });

            if (r.Статус == "Отмена")
                return BadRequest(new { message = "Аренда уже отменена." });

            r.Статус = "Отмена";
            await _db.SaveChangesAsync(ct);
            return Ok(new { rentalId = id, status = r.Статус });
        }

        private static string? BuildInventorySummary(Inventory i)
        {
            var parts = new[] { i.Лыжи, i.Сноуборд, i.Ботинки, i.Палки }
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Take(2);
            var s = string.Join(", ", parts);
            return string.IsNullOrEmpty(s) ? $"ID {i.ID_Инвентаря}" : s;
        }
    }
}
