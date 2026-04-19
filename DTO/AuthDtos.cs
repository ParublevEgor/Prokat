using System.ComponentModel.DataAnnotations;

namespace Prokat.API.DTO
{
    public class RegisterRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(64)]
        public string Login { get; set; } = "";

        [Required]
        [MinLength(4)]
        [MaxLength(128)]
        public string Password { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Required]
        public string FirstName { get; set; } = "";

        [Range(5, 99)]
        public int? Age { get; set; }

        public int? Height { get; set; }
        public int? Weight { get; set; }
        public int? ShoeSize { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        public string Login { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }

    public class UpdateProfileRequest
    {
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public int? Age { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }
        public int? ShoeSize { get; set; }
        public string? ProfilePhotoBase64 { get; set; }
        public bool RemoveProfilePhoto { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; } = "";
        public string Role { get; set; } = "";
        public string Login { get; set; } = "";
    }

    public class VatSettingsDto
    {
        public decimal VatRate { get; set; }
    }

    public class VatUpdateRequest
    {
        [Range(0, 1)]
        public decimal VatRate { get; set; }
    }
}
