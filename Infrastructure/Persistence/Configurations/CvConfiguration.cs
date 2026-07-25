using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class CvConfiguration : IEntityTypeConfiguration<Cv>
    {
        public void Configure(EntityTypeBuilder<Cv> builder)
        {
            builder.ToTable("cvs");

            builder.HasKey(cv => cv.Id);

            // One CV per seeker. CvBusinessRules used to enforce this with an extra round trip and
            // a thrown BusinessException; the database can simply guarantee it.
            builder.HasIndex(cv => cv.JobSeekerId).IsUnique();

            builder.Property(cv => cv.Information).HasMaxLength(4000);
            builder.Property(cv => cv.ImageUrl).HasMaxLength(512);
            builder.Property(cv => cv.Hobbies).HasMaxLength(1000);

            builder.Property(cv => cv.Skills).HasColumnType("text[]");
            builder.HasIndex(cv => cv.Skills).HasMethod("gin");

            // Owned type -> three inline nullable columns on cvs, not a separate table.
            builder.OwnsOne(cv => cv.SocialMedia, socialMedia =>
            {
                socialMedia.Property(media => media.Github).HasMaxLength(256).HasColumnName("social_github");
                socialMedia.Property(media => media.Linkedin).HasMaxLength(256).HasColumnName("social_linkedin");
                socialMedia.Property(media => media.WebSite).HasMaxLength(256).HasColumnName("social_website");
            });

            builder.Property<uint>("xmin").IsRowVersion();   // optimistic concurrency via Npgsql's system column

            // Matches the soft-delete filter on JobSeeker. Without it EF warns that a filtered
            // principal has an unfiltered required dependent — concretely, a CV belonging to a
            // soft-deleted seeker would still surface in queries while its owner does not.
            builder.HasQueryFilter(cv => cv.JobSeeker.DeletedAt == null);

            builder.HasMany(cv => cv.Educations).WithOne(education => education.Cv)
                .HasForeignKey(education => education.CvId).OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(cv => cv.JobExperiences).WithOne(experience => experience.Cv)
                .HasForeignKey(experience => experience.CvId).OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(cv => cv.Languages).WithOne(language => language.Cv)
                .HasForeignKey(language => language.CvId).OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(cv => cv.Projects).WithOne(project => project.Cv)
                .HasForeignKey(project => project.CvId).OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(cv => cv.Files).WithOne(file => file.Cv)
                .HasForeignKey(file => file.CvId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    // Each CV child repeats the parent's soft-delete filter. EF warns otherwise, and the warning is
    // right: a filtered principal with unfiltered required dependents means an education row would
    // still surface in queries after the CV — and the seeker — were soft-deleted.
    public class EducationConfiguration : IEntityTypeConfiguration<Education>
    {
        public void Configure(EntityTypeBuilder<Education> builder)
        {
            builder.ToTable("cv_educations");
            builder.HasKey(education => education.Id);
            builder.Property(education => education.School).HasMaxLength(200).IsRequired();
            builder.Property(education => education.Major).HasMaxLength(200).IsRequired();
            builder.Property(education => education.Grade).HasMaxLength(32);
            builder.HasIndex(education => education.CvId);
            builder.HasQueryFilter(education => education.Cv.JobSeeker.DeletedAt == null);
        }
    }

    public class JobExperienceConfiguration : IEntityTypeConfiguration<JobExperience>
    {
        public void Configure(EntityTypeBuilder<JobExperience> builder)
        {
            builder.ToTable("cv_job_experiences");
            builder.HasKey(experience => experience.Id);
            builder.Property(experience => experience.CompanyName).HasMaxLength(200).IsRequired();
            builder.Property(experience => experience.Department).HasMaxLength(200);
            builder.Property(experience => experience.Position).HasMaxLength(200).IsRequired();
            builder.Property(experience => experience.Description).HasMaxLength(2000);
            builder.HasIndex(experience => experience.CvId);
            builder.HasQueryFilter(experience => experience.Cv.JobSeeker.DeletedAt == null);
        }
    }

    public class CvLanguageConfiguration : IEntityTypeConfiguration<CvLanguage>
    {
        public void Configure(EntityTypeBuilder<CvLanguage> builder)
        {
            builder.ToTable("cv_languages");
            builder.HasKey(language => language.Id);
            builder.Property(language => language.Name).HasMaxLength(100).IsRequired();
            builder.Property(language => language.Level).HasConversion<string>().HasMaxLength(32).IsRequired();
            builder.HasIndex(language => language.CvId);
            builder.HasQueryFilter(language => language.Cv.JobSeeker.DeletedAt == null);
        }
    }

    public class CvProjectConfiguration : IEntityTypeConfiguration<CvProject>
    {
        public void Configure(EntityTypeBuilder<CvProject> builder)
        {
            builder.ToTable("cv_projects");
            builder.HasKey(project => project.Id);
            builder.Property(project => project.Name).HasMaxLength(200).IsRequired();
            builder.Property(project => project.Description).HasMaxLength(2000);
            builder.HasIndex(project => project.CvId);
            builder.HasQueryFilter(project => project.Cv.JobSeeker.DeletedAt == null);
        }
    }

    public class CvFileConfiguration : IEntityTypeConfiguration<CvFile>
    {
        public void Configure(EntityTypeBuilder<CvFile> builder)
        {
            builder.ToTable("cv_files");
            builder.HasKey(file => file.Id);
            builder.Property(file => file.FileName).HasMaxLength(260).IsRequired();
            builder.Property(file => file.StoragePath).HasMaxLength(1024).IsRequired();
            builder.Property(file => file.StorageProvider).HasConversion<string>().HasMaxLength(32).IsRequired();
            builder.Property(file => file.ContentType).HasMaxLength(128);

            // Not unique: a CV may carry several attachments.
            builder.HasIndex(file => file.CvId);
            builder.HasQueryFilter(file => file.Cv.JobSeeker.DeletedAt == null);
        }
    }

    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("employer_departments");
            builder.HasKey(department => department.Id);
            builder.Property(department => department.Name).HasMaxLength(200).IsRequired();
            builder.HasIndex(department => department.EmployerId);

            // Mirrors the Employer soft-delete filter, as above.
            builder.HasQueryFilter(department => department.Employer.DeletedAt == null);
        }
    }
}
