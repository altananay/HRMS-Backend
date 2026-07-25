using Domain.Common;

namespace Domain.Entities
{
    /// <summary>
    /// A rotating refresh token.
    /// </summary>
    /// <remarks>
    /// Entirely new. The pre-migration codebase had no refresh-token support of any kind — the
    /// string "refresh" did not appear anywhere in it. <c>AccessToken</c> carried only a token and
    /// an expiry, there was no refresh, logout or revoke endpoint, and the only way to renew a
    /// session was to re-post credentials.
    ///
    /// Only the SHA-256 hash of the token is stored. A database leak must not hand an attacker
    /// usable sessions, which is exactly what storing the raw token would do.
    /// </remarks>
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }

        /// <summary>SHA-256 of the token issued to the client. The raw value is never persisted.</summary>
        public string TokenHash { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public string? CreatedByIp { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? RevokedByIp { get; set; }

        /// <summary>
        /// The token issued in place of this one, forming a rotation chain.
        /// </summary>
        /// <remarks>
        /// Presenting an already-revoked token means it was replayed, which implies theft. The
        /// response is to walk this chain and revoke the lot — see the auth service in Phase 4.
        /// </remarks>
        public Guid? ReplacedById { get; set; }

        public User User { get; set; } = null!;

        public bool IsRevoked => RevokedAt is not null;

        public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;

        public bool IsActive(DateTime utcNow) => !IsRevoked && !IsExpired(utcNow);
    }
}
