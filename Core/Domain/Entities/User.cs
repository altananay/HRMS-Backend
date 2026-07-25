using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Shared identity for all three actor types, mapped table-per-type.
    /// </summary>
    /// <remarks>
    /// Collapses what used to be four Mongo collections with no referential integrity between them.
    /// Previously <c>User</c> was an empty marker entity whose only purpose was to mint an ObjectId
    /// that JobSeeker/Employer/SystemStaff then copied onto themselves:
    /// <code>var user = new User(); await _userService.Add(user); jobSeeker.Id = user.Id;</code>
    /// Two inserts, two collections, no foreign key, and nothing keeping them consistent. Real
    /// inheritance makes that a single insert with a single identity.
    ///
    /// Email uniqueness is enforced by a partial unique index (<c>WHERE deleted_at IS NULL</c>), so
    /// a soft-deleted account releases its address for reuse.
    /// </remarks>
    public abstract class User : BaseEntity, ISoftDeletable
    {
        protected User(UserType userType) => UserType = userType;

        public string Email { get; set; } = null!;

        /// <summary>
        /// PHC-style string with the algorithm, iteration count and salt embedded.
        /// </summary>
        /// <remarks>
        /// Replaces the <c>byte[] PasswordHash</c> + <c>byte[] PasswordSalt</c> pair produced by
        /// HashingHelper's single-round HMACSHA512, which had no work factor and was therefore not
        /// a password KDF at all.
        /// </remarks>
        public string PasswordHash { get; set; } = null!;

        /// <summary>
        /// Rotated on password change and on refresh-token reuse detection, which invalidates every
        /// outstanding refresh token for this user.
        /// </summary>
        public Guid SecurityStamp { get; set; } = Guid.NewGuid();

        /// <summary>Checked at login. The old code set this at registration and never read it.</summary>
        public bool IsActive { get; set; } = true;

        public UserType UserType { get; private set; }

        public DateTime? DeletedAt { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = [];

        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
