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
            var login = req.Login?.Trim() ?? "";
            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Логин == login, ct);
            if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.ПарольХеш))
                return Unauthorized(new { message = "Неверный логин или пароль." });

            var token = _jwt.CreateAccessToken(user.ID_Учетной_записи, user.Логин, user.Роль);
            return Ok(new AuthResponse { Token = token, Role = user.Роль, Login = user.Логин });
        }

        /// <summary>Данные из действительного JWT (после проверки подписи middleware).</summary>
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

            return Ok(new MeResponseDto
            {
                UserId = userId,
                Login = u.Логин,
                Role = u.Роль,
                ClientId = u.ID_Клиента,
            });
        }
    }
}
