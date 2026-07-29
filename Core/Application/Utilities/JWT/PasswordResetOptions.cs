using System.ComponentModel.DataAnnotations;

namespace Application.Utilities.JWT
{
    public class PasswordResetOptions
    {
        public const string SectionName = "PasswordReset";

        /// <summary>
        /// Short on purpose: this token bypasses the password entirely, so it lives minutes rather
        /// than the refresh token's days.
        /// </summary>
        [Range(5, 1440)]
        public int TokenLifetimeMinutes { get; set; } = 60;

        /// <summary>
        /// Where the emailed link points — the frontend's reset screen, not an API route.
        /// </summary>
        /// <remarks>
        /// Configured rather than derived from the incoming request: building it from Host headers
        /// is how host-header poisoning turns a reset mail into a token-harvesting link.
        /// </remarks>
        [Required]
        public string LinkBaseUrl { get; set; } = "http://localhost:3000/reset-password";
    }
}
