using Application.Abstractions;
using Application.Utilities.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Persistence.Seeding
{
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

            logger.LogInformation("Seeded bootstrap administrator {Email}", email);
        }
    }
}
