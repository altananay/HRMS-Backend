using System.Reflection;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

using Persistence.ValueGeneration;

namespace Persistence
{
    public class HrmsDbContext : DbContext
    {
        public HrmsDbContext(DbContextOptions<HrmsDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<JobSeeker> JobSeekers => Set<JobSeeker>();
        public DbSet<Employer> Employers => Set<Employer>();
        public DbSet<SystemStaff> SystemStaff => Set<SystemStaff>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Cv> Cvs => Set<Cv>();
        public DbSet<Education> Educations => Set<Education>();
        public DbSet<JobExperience> JobExperiences => Set<JobExperience>();
        public DbSet<CvLanguage> CvLanguages => Set<CvLanguage>();
        public DbSet<CvProject> CvProjects => Set<CvProject>();
        public DbSet<CvFile> CvFiles => Set<CvFile>();

        public DbSet<JobPosition> JobPositions => Set<JobPosition>();
        public DbSet<JobAdvertisement> JobAdvertisements => Set<JobAdvertisement>();
        public DbSet<JobApplication> JobApplications => Set<JobApplication>();
        public DbSet<Contact> Contacts => Set<Contact>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("citext");

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Every BaseEntity key is generated on insert, by us, as a version 7 GUID. Applied here
            // in one place rather than repeated in twelve configurations — and applied *after* the
            // configurations so it cannot be forgotten when an entity is added.
            //
            // The key must stay unset until save. EF decides whether an entity it finds inside a
            // tracked parent's collection is new or already persisted by asking whether its key is
            // set, so pre-filling it in the entity's constructor made every new child look like an
            // existing row. See GuidV7ValueGenerator.
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    continue;
                }

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.Id))
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<GuidV7ValueGenerator>();
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
