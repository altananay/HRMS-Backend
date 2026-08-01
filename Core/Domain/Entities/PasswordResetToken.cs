using Domain.Common;

namespace Domain.Entities
{
    public class PasswordResetToken : BaseEntity
    {
        public Guid UserId { get; set; }

        public string TokenHash { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public DateTime? UsedAt { get; set; }

        public User User { get; set; } = null!;

        public bool IsUsed => UsedAt is not null;

        public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;

        public bool IsActive(DateTime utcNow) => !IsUsed && !IsExpired(utcNow);
    }
}
