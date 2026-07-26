using System.ComponentModel.DataAnnotations;

namespace Application.Utilities.JWT
{
    public class TokenOptions
    {
        [Required]
        public string Issuer { get; set; } = null!;

        [Required]
        public string Audience { get; set; } = null!;

        [Required]
        [MinLength(32, ErrorMessage = "TokenOptions:SecurityKey must be at least 32 characters.")]
        public string SecurityKey { get; set; } = null!;

        [Range(1, 1440)]
        public int AccessTokenExpirationMinutes { get; set; } = 15;

        [Range(1, 365)]
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }
}
