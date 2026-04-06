using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Prokat.API.Services
{
    /// <summary>Выпуск подписанных JWT (алгоритм HS256). Полезная нагрузка не шифруется — проверяется подпись на сервере.</summary>
    public interface IJwtTokenService
    {
        string CreateAccessToken(int userId, string username, string role);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public string CreateAccessToken(int userId, string username, string role)
        {
            var jwt = _config.GetSection("Jwt");
            var keyStr = jwt["Key"] ?? "";
            if (Encoding.UTF8.GetByteCount(keyStr) < 32)
                throw new InvalidOperationException(
                    "В конфигурации Jwt:Key нужна строка не короче 32 байт в UTF-8 (для HMAC-SHA256).");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
