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
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtTokenService _jwt;

        public AuthController(ApplicationDbContext db, IJwtTokenService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
        {
            var login = req.Login.Trim();
            if (await _db.AppUsers.AnyAsync(u => u.Логин == login, ct))
                return BadRequest(new { message = "Такой логин уже занят." });

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var client = new Client
            {
                Фамилия = req.LastName,
                Имя = req.FirstName,
                Возраст = req.Age,
                Рост = req.Height,
                Вес = req.Weight,
                РазмерОбуви = req.ShoeSize,
            };
            _db.Clients.Add(client);
            await _db.SaveChangesAsync(ct);

            var user = new AppUser
            {
                Логин = login,
                ПарольХеш = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Роль = "User",
                ID_Клиента = client.ID_Клиента,
            };
            _db.AppUsers.Add(user);
            await _db.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);

            var token = _jwt.CreateAccessToken(user.ID_Учетной_записи, user.Логин, user.Роль);
            return Ok(new AuthResponse { Token = token, Role = user.Роль, Login = user.Логин });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
        {
            return await LoginInternal(req, null, ct);
        }

        [HttpPost("login/user")]
        public async Task<IActionResult> LoginUser([FromBody] LoginRequest req, CancellationToken ct)
        {
            return await LoginInternal(req, "User", ct);
        }

        [HttpPost("login/admin")]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginRequest req, CancellationToken ct)
        {
            return await LoginInternal(req, "Admin", ct);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var userId))
                return Unauthorized();

            var u = await _db.AppUsers.AsNoTracking()
                .Where(x => x.ID_Учетной_записи == userId)
                .Select(x => new { x.Логин, x.Роль, x.ID_Клиента })
                .FirstOrDefaultAsync(ct);

            if (u is null)
                return Unauthorized();

            var profile = u.ID_Клиента is int cid
                ? await _db.Clients.AsNoTracking()
                    .Where(c => c.ID_Клиента == cid)
                    .Select(c => new { c.Фамилия, c.Имя, c.Возраст, c.Рост, c.Вес, c.РазмерОбуви, c.ФотоПрофиля })
                    .FirstOrDefaultAsync(ct)
                : null;

            return Ok(new MeResponseDto
            {
                UserId = userId,
                Login = u.Логин,
                Role = u.Роль,
                ClientId = u.ID_Клиента,
                LastName = profile?.Фамилия,
                FirstName = profile?.Имя,
                Age = profile?.Возраст,
                Height = profile?.Рост,
                Weight = profile?.Вес,
                ShoeSize = profile?.РазмерОбуви,
                ProfilePhotoBase64 = profile?.ФотоПрофиля,
            });
        }

        [Authorize(Roles = "User")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest req, CancellationToken ct)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var userId))
                return Unauthorized();

            var acc = await _db.AppUsers.FirstOrDefaultAsync(x => x.ID_Учетной_записи == userId, ct);
            if (acc is null || acc.ID_Клиента is null)
                return BadRequest(new { message = "Профиль клиента не найден." });

            var client = await _db.Clients.FirstOrDefaultAsync(c => c.ID_Клиента == acc.ID_Клиента.Value, ct);
            if (client is null)
                return BadRequest(new { message = "Профиль клиента не найден." });

            if (req.LastName is not null) client.Фамилия = req.LastName.Trim();
            if (req.FirstName is not null) client.Имя = req.FirstName.Trim();
            if (req.Age.HasValue) client.Возраст = req.Age;
            if (req.Height.HasValue) client.Рост = req.Height;
            if (req.Weight.HasValue) client.Вес = req.Weight;
            if (req.ShoeSize.HasValue) client.РазмерОбуви = req.ShoeSize;
            if (req.RemoveProfilePhoto)
                client.ФотоПрофиля = null;
            else if (!string.IsNullOrWhiteSpace(req.ProfilePhotoBase64))
                client.ФотоПрофиля = req.ProfilePhotoBase64;

            await _db.SaveChangesAsync(ct);
            return Ok(new { message = "Профиль обновлён." });
        }

        private async Task<IActionResult> LoginInternal(LoginRequest req, string? requiredRole, CancellationToken ct)
        {
            var login = req.Login?.Trim() ?? "";
            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Логин == login, ct);
            if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.ПарольХеш))
                return Unauthorized(new { message = "Неверный логин или пароль." });

            if (!string.IsNullOrEmpty(requiredRole) && !string.Equals(user.Роль, requiredRole, StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { message = $"Этот вход доступен только для роли {requiredRole}." });

            var token = _jwt.CreateAccessToken(user.ID_Учетной_записи, user.Логин, user.Роль);
            return Ok(new AuthResponse { Token = token, Role = user.Роль, Login = user.Логин });
        }
    }
}
