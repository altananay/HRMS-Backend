using Domain.Common;

namespace Domain.Entities
{
    /// <summary>
    /// A single-use, time-limited grant to set a new password without knowing the old one.
    /// </summary>
    /// <remarks>
    /// Deliberately shaped like <see cref="RefreshToken"/>: the raw value goes to the user, only its
    /// SHA-256 hash is stored, and a leaked database gives an attacker nothing usable. The lifetime
    /// is far shorter than a refresh token's because this one bypasses the password entirely.
    /// </remarks>
    public class PasswordResetToken : BaseEntity
    {
        public Guid UserId { get; set; }

        public string TokenHash { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        /// <summary>Stamped on first successful use. A reset link works exactly once.</summary>
        public DateTime? UsedAt { get; set; }

        public User User { get; set; } = null!;

        public bool IsUsed => UsedAt is not null;

        public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;

        public bool IsActive(DateTime utcNow) => !IsUsed && !IsExpired(utcNow);
    }
}
