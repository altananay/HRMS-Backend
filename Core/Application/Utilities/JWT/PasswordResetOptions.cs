using System.ComponentModel.DataAnnotations;

namespace Application.Utilities.JWT
{
    public class PasswordResetOptions
    {
        public const string SectionName = "PasswordReset";

        [Range(5, 1440)]
        public int TokenLifetimeMinutes { get; set; } = 60;

        [Required]
        public string LinkBaseUrl { get; set; } = "http://localhost:3000/reset-password";
    }
}
