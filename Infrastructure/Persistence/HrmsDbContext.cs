using System.Reflection;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
            // Case-insensitive text, used for Email, Role.Name and JobPosition.Name. This removes a
            // whole class of "logged in with Altan@x.com but registered altan@x.com" bugs at the
            // database level, and is cleaner than carrying a NormalizedEmail shadow column.
            modelBuilder.HasPostgresExtension("citext");

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}
