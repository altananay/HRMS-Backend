using System.ComponentModel.DataAnnotations;

namespace Application.Utilities.JWT
{
    /// <summary>
    /// JWT settings, bound from configuration and validated at startup.
    /// </summary>
    /// <remarks>
    /// The annotations here are what <c>ValidateDataAnnotations().ValidateOnStart()</c> in
    /// Program.cs enforces. This type previously had no validation, so on a clean clone
    /// <c>GetSection("TokenOptions").Get&lt;TokenOptions&gt;()</c> returned <c>null</c> and startup
    /// died on <c>tokenOptions.Issuer</c> with a NullReferenceException that pointed nowhere near
    /// the real cause — missing configuration.
    /// </remarks>
    public class TokenOptions
    {
        [Required]
        public string Issuer { get; set; } = null!;

        [Required]
        public string Audience { get; set; } = null!;

        /// <summary>
        /// HMAC-SHA256 signing key. At least 32 characters.
        /// </summary>
        /// <remarks>
        /// The old signing helper passed <c>SecurityAlgorithms.HmacSha512Signature</c> — the XMLDSig
        /// URI rather than the JWS <c>alg</c> value — which imposed a 64-byte key requirement for no
        /// benefit.
        /// </remarks>
        [Required]
        [MinLength(32, ErrorMessage = "TokenOptions:SecurityKey must be at least 32 characters.")]
        public string SecurityKey { get; set; } = null!;

        [Range(1, 1440)]
        public int AccessTokenExpirationMinutes { get; set; } = 15;

        [Range(1, 365)]
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }
}
