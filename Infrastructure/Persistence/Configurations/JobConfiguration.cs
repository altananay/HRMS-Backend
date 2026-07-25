using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class JobPositionConfiguration : IEntityTypeConfiguration<JobPosition>
    {
        public void Configure(EntityTypeBuilder<JobPosition> builder)
        {
            builder.ToTable("job_positions");

            builder.HasKey(position => position.Id);

            builder.Property(position => position.Name)
                .HasColumnType("citext")
                .HasMaxLength(200)
                .IsRequired();

            // The constraint that turns this from a per-advertisement field into a real lookup.
            builder.HasIndex(position => position.Name).IsUnique();
        }
    }

    public class JobAdvertisementConfiguration : IEntityTypeConfiguration<JobAdvertisement>
    {
        public void Configure(EntityTypeBuilder<JobAdvertisement> builder)
        {
            builder.ToTable("job_advertisements");

            builder.HasKey(advertisement => advertisement.Id);

            builder.Property(advertisement => advertisement.Title).HasMaxLength(200).IsRequired();
            builder.Property(advertisement => advertisement.Description).HasMaxLength(8000).IsRequired();
            builder.Property(advertisement => advertisement.Experience).HasMaxLength(100);
            builder.Property(advertisement => advertisement.City).HasMaxLength(100);
            builder.Property(advertisement => advertisement.Currency).HasMaxLength(3);

            builder.Property(advertisement => advertisement.JobType)
                .HasConversion<string>().HasMaxLength(32).IsRequired();

            // numeric(18,2), not double: salaries are money and must not carry binary rounding error.
            builder.Property(advertisement => advertisement.MinSalary).HasPrecision(18, 2);
            builder.Property(advertisement => advertisement.MaxSalary).HasPrecision(18, 2);

            builder.Property(advertisement => advertisement.Skills).HasColumnType("text[]");
            builder.HasIndex(advertisement => advertisement.Skills).HasMethod("gin");

            builder.HasIndex(advertisement => advertisement.EmployerId);
            builder.HasIndex(advertisement => new { advertisement.IsActive, advertisement.Deadline });
            builder.HasIndex(advertisement => advertisement.City);

            builder.ToTable(table =>
            {
                table.HasCheckConstraint("ck_job_advertisements_salary_range",
                    "max_salary IS NULL OR min_salary IS NULL OR max_salary >= min_salary");
                table.HasCheckConstraint("ck_job_advertisements_open_positions",
                    "open_positions > 0");
            });

            builder.Property<uint>("xmin").IsRowVersion();   // optimistic concurrency via Npgsql's system column

            builder.HasQueryFilter(advertisement => advertisement.DeletedAt == null);

            // RESTRICT, not CASCADE: removing a shared lookup row must never quietly delete every
            // advertisement referencing it. The admin delete endpoint surfaces this as a 409.
            builder.HasOne(advertisement => advertisement.JobPosition)
                .WithMany(position => position.JobAdvertisements)
                .HasForeignKey(advertisement => advertisement.JobPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(advertisement => advertisement.JobApplications)
                .WithOne(application => application.JobAdvertisement)
                .HasForeignKey(application => application.JobAdvertisementId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
    {
        public void Configure(EntityTypeBuilder<JobApplication> builder)
        {
            builder.ToTable("job_applications");

            builder.HasKey(application => application.Id);

            builder.Property(application => application.JobSeekerNote).HasMaxLength(2000);
            builder.Property(application => application.EmployerNote).HasMaxLength(2000);

            builder.Property(application => application.Status)
                .HasConversion<string>().HasMaxLength(32).IsRequired();

            // Nothing stopped a seeker applying to the same advertisement repeatedly before.
            builder.HasIndex(application => new { application.JobSeekerId, application.JobAdvertisementId })
                .IsUnique();

            builder.HasIndex(application => application.JobAdvertisementId);

            // Both principals are soft-deletable, so the filter has to cover both — otherwise an
            // application would outlive the advertisement or the seeker it belongs to in queries.
            builder.HasQueryFilter(application =>
                application.JobAdvertisement.DeletedAt == null && application.JobSeeker.DeletedAt == null);

            builder.Property<uint>("xmin").IsRowVersion();   // optimistic concurrency via Npgsql's system column
        }
    }

    public class ContactConfiguration : IEntityTypeConfiguration<Contact>
    {
        public void Configure(EntityTypeBuilder<Contact> builder)
        {
            builder.ToTable("contacts");

            builder.HasKey(contact => contact.Id);

            builder.Property(contact => contact.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(contact => contact.LastName).HasMaxLength(100).IsRequired();
            builder.Property(contact => contact.Email).HasMaxLength(256).IsRequired();
            builder.Property(contact => contact.Subject).HasMaxLength(200).IsRequired();
            builder.Property(contact => contact.Message).HasMaxLength(4000).IsRequired();

            builder.HasIndex(contact => contact.IsHandled);
        }
    }
}
