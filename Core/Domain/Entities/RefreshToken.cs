using Domain.Common;

namespace Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }

        public string TokenHash { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public string? CreatedByIp { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? RevokedByIp { get; set; }

        public Guid? ReplacedById { get; set; }

        public User User { get; set; } = null!;

        public bool IsRevoked => RevokedAt is not null;

        public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;

        public bool IsActive(DateTime utcNow) => !IsRevoked && !IsExpired(utcNow);
    }
}
