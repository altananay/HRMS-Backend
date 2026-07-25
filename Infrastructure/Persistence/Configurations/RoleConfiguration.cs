using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("roles");

            builder.HasKey(role => role.Id);

            builder.Property(role => role.Name)
                .HasColumnType("citext")
                .HasMaxLength(64)
                .IsRequired();

            builder.HasIndex(role => role.Name).IsUnique();
        }
    }

    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("user_roles");

            builder.HasKey(userRole => new { userRole.UserId, userRole.RoleId });

            builder.HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens");

            builder.HasKey(token => token.Id);

            builder.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();

            // Lookup on refresh is by hash, so it must be indexed; unique because two live tokens
            // can never legitimately hash to the same value.
            builder.HasIndex(token => token.TokenHash).IsUnique();

            // Covers "revoke everything for this user" and the expiry sweep.
            builder.HasIndex(token => new { token.UserId, token.ExpiresAt });

            builder.Property(token => token.CreatedByIp).HasMaxLength(45);
            builder.Property(token => token.RevokedByIp).HasMaxLength(45);

            builder.Ignore(token => token.IsRevoked);

            // A soft-deleted user's tokens must stop resolving, or a deleted account could keep
            // refreshing its session indefinitely.
            builder.HasQueryFilter(token => token.User.DeletedAt == null);
        }
    }
}
