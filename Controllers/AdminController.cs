using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using Prokat.API.Data;

using Prokat.API.DTO;

using Prokat.API.Models;

using Prokat.API.Services;

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

            var users = await _db.AppUsers.AsNoTracking()

                .OrderBy(u => u.ID_Учетной_записи)

                .ToListAsync(ct);



            /// Окна аренды в БД задаются как локальное время смены (см. RentalWindowHelper).

            var now = DateTime.Now;

            var list = new List<AdminUserDto>(users.Count);

            foreach (var u in users)

            {

                var dto = new AdminUserDto

                {

                    Id = u.ID_Учетной_записи,

                    Login = u.Логин,

                    Role = u.Роль,

                    ClientId = u.ID_Клиента,

                    CanDelete = u.Роль != "Admin",

                };



                if (u.Роль == "Admin")

                {

                    dto.DeleteBlockedReason = "Нельзя удалить администратора.";

                    list.Add(dto);

                    continue;

                }



                if (u.ID_Клиента is int clientId)

                {

                    var hasActiveRentals = await (

                        from r in _db.RentalBookings.AsNoTracking()

                        join o in _db.Orders.AsNoTracking() on r.ID_Заказа equals o.ID_Заказа

                        where o.ID_Клиента == clientId

                              && r.Статус != "Отмена"

                              && r.ДатаНачала <= now

                              && now < r.ДатаОкончания

                        select r.ID_Аренды

                    ).AnyAsync(ct);



                    if (hasActiveRentals)

                    {

                        dto.CanDelete = false;

                        dto.DeleteBlockedReason = "Есть текущая аренда.";

                    }

                    else

                    {

                        var hasUnpaidOrders = await _db.Orders.AsNoTracking()

                            .AnyAsync(o => o.ID_Клиента == clientId && (o.Сумма_оплаты == null || o.Сумма_оплаты <= 0), ct);



                        if (hasUnpaidOrders)

                        {

                            dto.CanDelete = false;

                            dto.DeleteBlockedReason = "Есть неоплаченные счета.";

                        }

                    }

                }



                list.Add(dto);

            }



            return Ok(list);

        }



        [HttpGet("users/{id:int}/detail")]

        public async Task<IActionResult> GetUserDetail(int id, CancellationToken ct)

        {

            var u = await _db.AppUsers.AsNoTracking()

                .FirstOrDefaultAsync(x => x.ID_Учетной_записи == id, ct);

            if (u is null)

                return NotFound(new { message = "Пользователь не найден." });



            var dto = new AdminUserDetailDto

            {

                UserId = u.ID_Учетной_записи,

                Login = u.Логин,

                Role = u.Роль,

                ClientId = u.ID_Клиента,

            };



            if (u.ID_Клиента is int cid)

            {

                var c = await _db.Clients.AsNoTracking()

                    .FirstOrDefaultAsync(x => x.ID_Клиента == cid, ct);

                if (c is not null)

                {

                    dto.LastName = c.Фамилия;

                    dto.FirstName = c.Имя;

                    dto.Age = c.Возраст;

                    dto.Height = c.Рост;

                    dto.Weight = c.Вес;

                    dto.ShoeSize = c.РазмерОбуви;

                    dto.Deposit = c.Залог;

                    dto.HasProfilePhoto = !string.IsNullOrEmpty(c.ФотоПрофиля);

                }

            }



            return Ok(dto);

        }



        [HttpGet("users/undo-status")]

        public IActionResult UndoStatus()

        {

            return Ok(new { canUndo = AdminDeletionUndo.CanUndo });

        }



        [HttpPost("users/restore")]

        public async Task<IActionResult> RestoreLastUser(CancellationToken ct)

        {

            var snap = AdminDeletionUndo.Peek();

            if (snap is null)

                return BadRequest(new { message = "Нет удалённой учётной записи для восстановления." });



            if (await _db.AppUsers.AsNoTracking().AnyAsync(u => u.Логин == snap.Login, ct))

                return BadRequest(new { message = "Логин уже занят — восстановление невозможно." });



            _db.AppUsers.Add(new AppUser

            {

                Логин = snap.Login,

                ПарольХеш = snap.PasswordHash,

                Роль = snap.Role,

                ID_Клиента = snap.ClientId,

            });

            await _db.SaveChangesAsync(ct);

            AdminDeletionUndo.Clear();

            return Ok(new { restored = true });

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



            if (target.ID_Клиента is int clientId)

            {

                var now = DateTime.Now;

                var hasActiveRentals = await (

                    from r in _db.RentalBookings

                    join o in _db.Orders on r.ID_Заказа equals o.ID_Заказа

                    where o.ID_Клиента == clientId

                          && r.Статус != "Отмена"

                          && r.ДатаНачала <= now

                          && now < r.ДатаОкончания

                    select r.ID_Аренды

                ).AnyAsync(ct);



                if (hasActiveRentals)

                    return BadRequest(new { message = "Нельзя удалить: у пользователя есть текущая аренда." });



                var hasUnpaidOrders = await _db.Orders

                    .AnyAsync(o => o.ID_Клиента == clientId && (o.Сумма_оплаты == null || o.Сумма_оплаты <= 0), ct);

                if (hasUnpaidOrders)

                    return BadRequest(new { message = "Нельзя удалить: есть неоплаченные счета." });

            }



            AdminDeletionUndo.Remember(new UserDeletionSnapshot

            {

                Login = target.Логин,

                PasswordHash = target.ПарольХеш,

                Role = target.Роль,

                ClientId = target.ID_Клиента,

            });



            _db.AppUsers.Remove(target);

            await _db.SaveChangesAsync(ct);

            return Ok(new { canUndo = true });

        }



        [HttpGet("rentals")]

        public async Task<IActionResult> AllRentals(CancellationToken ct)

        {

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

                join c in _db.Clients on o.ID_Клиента equals c.ID_Клиента into cg

                from c in cg.DefaultIfEmpty()

                orderby r.ДатаНачала descending

                select new AdminRentalDto

                {

                    RentalId = r.ID_Аренды,

                    ClientName = c != null ? (c.Фамилия + " " + c.Имя).Trim() : null,

                    Start = r.ДатаНачала,

                    End = r.ДатаОкончания,

                    Status = r.Статус,

                    InventorySummary = BuildInventorySummary(i),

                    Total = o.Сумма_оплаты,

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



        [HttpGet("inventory/status")]

        public async Task<IActionResult> InventoryStatus(CancellationToken ct)

        {

            var now = DateTime.Now;

            var rentedIds = await _db.RentalBookings.AsNoTracking()

                .Where(r => r.Статус != "Отмена" && r.ДатаНачала <= now && now < r.ДатаОкончания)

                .Select(r => r.ID_Инвентаря)

                .Distinct()

                .ToListAsync(ct);



            var rentedSet = rentedIds.ToHashSet();

            var items = await _db.Inventory.AsNoTracking()
                .Include(i => i.Лыжи)
                .Include(i => i.Сноуборд)
                .Include(i => i.Ботинки)
                .Include(i => i.Палки)
                .Include(i => i.Шлем)
                .Include(i => i.Очки)
                .OrderBy(i => i.ID_Инвентаря)
                .ToListAsync(ct);



            var rows = items.Select(i => new AdminInventoryStatusDto

            {

                InventoryId = i.ID_Инвентаря,

                Type = i.ID_Лыжи != null ? "Лыжи" : (i.ID_Сноуборд != null ? "Сноуборд" : "Не определен"),
                Skis = i.Лыжи == null ? null : $"{i.Лыжи.Название} ({i.Лыжи.Тип}, {i.Лыжи.РостовкаСм} см)",
                Snowboard = i.Сноуборд == null ? null : $"{i.Сноуборд.Название} ({i.Сноуборд.Тип}, {i.Сноуборд.РостовкаСм} см)",
                Boots = i.Ботинки == null ? null : $"{i.Ботинки.Название}, EU {i.Ботинки.РазмерEU}",
                Poles = i.Палки == null ? null : $"{i.Палки.Название} ({i.Палки.ДлинаСм} см)",

                Status = rentedSet.Contains(i.ID_Инвентаря) ? "В аренде" : "В наличии",

            }).ToList();



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


