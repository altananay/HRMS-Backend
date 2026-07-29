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

            builder.HasQueryFilter(userRole => userRole.User.DeletedAt == null);

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

            builder.HasIndex(token => token.TokenHash).IsUnique();

            builder.HasIndex(token => new { token.UserId, token.ExpiresAt });

            builder.Property(token => token.CreatedByIp).HasMaxLength(45);
            builder.Property(token => token.RevokedByIp).HasMaxLength(45);

            builder.Ignore(token => token.IsRevoked);

            builder.HasQueryFilter(token => token.User.DeletedAt == null);
        }
    }

    public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
    {
        public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
        {
            builder.ToTable("password_reset_tokens");

            builder.HasKey(token => token.Id);

            builder.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();

            builder.HasIndex(token => token.TokenHash).IsUnique();

            // The lookup for "invalidate this user's outstanding links" on a successful reset.
            builder.HasIndex(token => new { token.UserId, token.ExpiresAt });

            builder.Ignore(token => token.IsUsed);

            builder.HasOne(token => token.User)
                .WithMany()
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(token => token.User.DeletedAt == null);
        }
    }
}
