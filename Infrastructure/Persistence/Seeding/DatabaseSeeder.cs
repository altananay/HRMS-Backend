using Application.Abstractions;
using Application.Utilities.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Persistence.Seeding
{
    /// <summary>
    /// Creates the role rows and, if no administrator exists, a bootstrap admin account.
    /// </summary>
    /// <remarks>
    /// Without this the system has no reachable administrator at all. Previously the only way to get
    /// one was to insert a document straight into MongoDB: creating staff required
    /// <c>[SecuredOperation("admin")]</c>, and signing in as staff called a method carrying the same
    /// attribute — so an admin was needed to create the first admin, and to log in as one.
    ///
    /// Idempotent: safe to run on every startup and in every test.
    /// </remarks>
    public static class DatabaseSeeder
    {
        private static readonly string[] RoleNames = [Roles.JobSeeker, Roles.Employer, Roles.Admin];

        public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<HrmsDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DatabaseSeeder));

            await SeedRolesAsync(context, cancellationToken);
            await SeedAdminAsync(context, hasher, configuration, logger, cancellationToken);
        }

        private static async Task SeedRolesAsync(HrmsDbContext context, CancellationToken cancellationToken)
        {
            var existing = await context.Roles
                .Select(role => role.Name)
                .ToListAsync(cancellationToken);

            var missing = RoleNames
                .Where(name => !existing.Contains(name, StringComparer.OrdinalIgnoreCase))
                .Select(name => new Role { Name = name })
                .ToList();

            if (missing.Count != 0)
            {
                context.Roles.AddRange(missing);
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        private static async Task SeedAdminAsync(
            HrmsDbContext context,
            IPasswordHasher hasher,
            IConfiguration configuration,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var adminRole = await context.Roles
                .FirstAsync(role => role.Name == Roles.Admin, cancellationToken);

            var adminExists = await context.UserRoles
                .AnyAsync(userRole => userRole.RoleId == adminRole.Id, cancellationToken);

            if (adminExists)
            {
                return;
            }

            var email = configuration["Seed:AdminEmail"];
            var password = configuration["Seed:AdminPassword"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                // Not fatal: a deployment may intend to create its administrator another way. But it
                // must be loud, because the alternative is an application nobody can administer.
                logger.LogWarning(
                    "No administrator exists and Seed:AdminEmail / Seed:AdminPassword are not configured. " +
                    "No admin account was created, so admin-only endpoints are unreachable.");
                return;
            }

            var admin = new SystemStaff
            {
                Email = email,
                PasswordHash = hasher.Hash(password),
                FirstName = "System",
                LastName = "Administrator"
            };

            context.SystemStaff.Add(admin);
            context.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });

            await context.SaveChangesAsync(cancellationToken);

            // The address is safe to log; the password obviously is not.
            logger.LogInformation("Seeded bootstrap administrator {Email}", email);
        }
    }
}
