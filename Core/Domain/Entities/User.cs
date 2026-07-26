using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public abstract class User : BaseEntity, ISoftDeletable
    {
        protected User(UserType userType) => UserType = userType;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public Guid SecurityStamp { get; set; } = Guid.NewGuid();

        public bool IsActive { get; set; } = true;

        public UserType UserType { get; private set; }

        public DateTime? DeletedAt { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = [];

        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
