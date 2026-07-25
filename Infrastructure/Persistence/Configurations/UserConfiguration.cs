using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(user => user.Id);

            builder.Property(user => user.Email)
                .HasColumnType("citext")
                .HasMaxLength(256)
                .IsRequired();

            // Partial unique index: a soft-deleted account releases its email for reuse. A plain
            // unique index would permanently burn the address of every deleted user.
            builder.HasIndex(user => user.Email)
                .IsUnique()
                .HasFilter("deleted_at IS NULL");

            builder.Property(user => user.PasswordHash).IsRequired();

            builder.Property(user => user.UserType)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(user => user.IsActive).HasDefaultValue(true);

            // xmin is a PostgreSQL system column, so optimistic concurrency costs no extra storage.
            // A stale update now raises DbUpdateConcurrencyException (mapped to 409) instead of
            // silently winning, which is what the old read-then-replace-whole-document flow did.
            builder.Property<uint>("xmin").IsRowVersion();   // optimistic concurrency via Npgsql's system column

            builder.HasQueryFilter(user => user.DeletedAt == null);

            builder.HasMany(user => user.UserRoles)
                .WithOne(userRole => userRole.User)
                .HasForeignKey(userRole => userRole.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(user => user.RefreshTokens)
                .WithOne(token => token.User)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class JobSeekerConfiguration : IEntityTypeConfiguration<JobSeeker>
    {
        public void Configure(EntityTypeBuilder<JobSeeker> builder)
        {
            // Table-per-type: job_seekers shares its primary key with users. TPH was rejected
            // because the three subtypes are almost column-disjoint, so it would force
            // employers.company_name and friends to become nullable, losing real NOT NULL guarantees.
            builder.ToTable("job_seekers");

            builder.Property(seeker => seeker.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(seeker => seeker.LastName).HasMaxLength(100).IsRequired();
            builder.Property(seeker => seeker.NationalId).HasMaxLength(11);

            builder.HasIndex(seeker => seeker.NationalId)
                .IsUnique()
                .HasFilter("national_id IS NOT NULL");

            builder.HasOne(seeker => seeker.Cv)
                .WithOne(cv => cv.JobSeeker)
                .HasForeignKey<Cv>(cv => cv.JobSeekerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(seeker => seeker.JobApplications)
                .WithOne(application => application.JobSeeker)
                .HasForeignKey(application => application.JobSeekerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class EmployerConfiguration : IEntityTypeConfiguration<Employer>
    {
        public void Configure(EntityTypeBuilder<Employer> builder)
        {
            builder.ToTable("employers");

            builder.Property(employer => employer.CompanyName).HasMaxLength(200).IsRequired();
            builder.Property(employer => employer.CompanyPhone).HasMaxLength(32);
            builder.Property(employer => employer.WebSite).HasMaxLength(256);
            builder.Property(employer => employer.Description).HasMaxLength(4000);

            // Npgsql maps string[] to text[] natively. A lookup table would be over-engineering for
            // free-text tags with no admin UI curating them.
            builder.Property(employer => employer.Sectors).HasColumnType("text[]");

            builder.HasMany(employer => employer.Departments)
                .WithOne(department => department.Employer)
                .HasForeignKey(department => department.EmployerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(employer => employer.JobAdvertisements)
                .WithOne(advertisement => advertisement.Employer)
                .HasForeignKey(advertisement => advertisement.EmployerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class SystemStaffConfiguration : IEntityTypeConfiguration<SystemStaff>
    {
        public void Configure(EntityTypeBuilder<SystemStaff> builder)
        {
            builder.ToTable("system_staff");

            builder.Property(staff => staff.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(staff => staff.LastName).HasMaxLength(100).IsRequired();
        }
    }
}
