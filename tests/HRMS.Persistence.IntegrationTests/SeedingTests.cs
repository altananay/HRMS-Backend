using Application.Utilities.Constants;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Persistence.Seeding;

namespace HRMS.Persistence.IntegrationTests;

[Collection(PersistenceCollection.Name)]
public class SeedingTests(PostgresFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await fixture.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Seeding_Should_CreateTheThreeRoles()
    {
        await DatabaseSeeder.SeedAsync(fixture.Services);

        await using var context = fixture.CreateContext();
        var roles = await context.Roles.Select(role => role.Name).ToListAsync();

        roles.ShouldContain(Roles.JobSeeker);
        roles.ShouldContain(Roles.Employer);
        roles.ShouldContain(Roles.Admin);
    }

    [Fact]
    public async Task Seeding_Should_CreateTheAdministratorFromConfiguration()
    {
        await DatabaseSeeder.SeedAsync(fixture.Services);

        await using var context = fixture.CreateContext();
        var admin = await context.SystemStaff.SingleOrDefaultAsync(
            staff => staff.Email == PostgresFixture.AdminEmail);

        admin.ShouldNotBeNull();
        admin.UserType.ShouldBe(UserType.SystemStaff);

        admin.PasswordHash.ShouldNotBe(PostgresFixture.AdminPassword);
        admin.PasswordHash.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SeededAdministrator_Should_HoldTheAdminRole()
    {
        await DatabaseSeeder.SeedAsync(fixture.Services);

        await using var context = fixture.CreateContext();

        var roles = await context.UserRoles
            .Where(link => link.User.Email == PostgresFixture.AdminEmail)
            .Select(link => link.Role.Name)
            .ToListAsync();

        roles.ShouldContain(Roles.Admin);
    }

    [Fact]
    public async Task Seeding_Should_BeIdempotent()
    {
        await DatabaseSeeder.SeedAsync(fixture.Services);
        await Should.NotThrowAsync(() => DatabaseSeeder.SeedAsync(fixture.Services));

        await using var context = fixture.CreateContext();

        (await context.Roles.CountAsync()).ShouldBe(3);
        (await context.SystemStaff.CountAsync(staff => staff.Email == PostgresFixture.AdminEmail)).ShouldBe(1);
    }

    [Fact]
    public async Task Seeding_Should_CreateNoAdministrator_When_TheCredentialsAreNotConfigured()
    {
        await using var services = BuildProviderWithoutAdminCredentials();

        await Should.NotThrowAsync(() => DatabaseSeeder.SeedAsync(services));

        await using var context = fixture.CreateContext();

        (await context.Roles.CountAsync()).ShouldBe(3);
        (await context.SystemStaff.CountAsync()).ShouldBe(0);
    }

    private ServiceProvider BuildProviderWithoutAdminCredentials()
    {
        var connectionString = fixture.CreateContext().Database.GetConnectionString()!;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = connectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddPersistenceServices(configuration);
        services.AddSingleton<Application.Abstractions.IPasswordHasher,
            Infrastructure.Services.Security.IdentityPasswordHasher>();

        return services.BuildServiceProvider();
    }
}
