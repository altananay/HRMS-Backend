using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace HRMS.WebAPI.FunctionalTests;

/// <summary>
/// Boots the real API against a throwaway PostgreSQL container.
/// </summary>
/// <remarks>
/// One container per test assembly, not per test: starting Postgres costs seconds, truncating tables
/// costs milliseconds. Per-test isolation comes from Respawn rather than from a transaction, because
/// the API runs on its own connections and would never see an uncommitted outer transaction.
///
/// Storage is forced to Local and identity verification to Null, so nothing here reaches Cloudflare
/// or a government SOAP endpoint.
/// </remarks>
public sealed class HrmsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("hrms_tests")
        .WithUsername("hrms")
        .WithPassword("hrms")
        .Build();

    private Respawner _respawner = null!;
    private NpgsqlConnection _connection = null!;

    /// <summary>Seeded administrator, used by tests that need an admin token.</summary>
    public const string AdminEmail = "admin@hrms.test";

    public const string AdminPassword = "Adm!nTest12345";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Program.cs applies migrations and seeds only in Development or Testing.
        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Publishes test configuration as environment variables before the host is built.
    /// </summary>
    /// <remarks>
    /// Not <c>ConfigureAppConfiguration</c>: under minimal hosting the <c>WebApplicationBuilder</c>
    /// in Program.cs reads its configuration while constructing the service collection — before the
    /// factory's <c>ConfigureWebHost</c> delegates are applied — so a connection string added there
    /// arrives too late and the DbContext is registered with an empty one.
    ///
    /// Environment variables are read by the default configuration sources, so they are in place no
    /// matter when the host happens to build. The double underscore is the standard section
    /// separator.
    /// </remarks>
    private void PublishTestConfiguration()
    {
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings__Postgres"] = _postgres.GetConnectionString(),

            ["TokenOptions__Issuer"] = "hrms-tests",
            ["TokenOptions__Audience"] = "hrms-tests",
            ["TokenOptions__SecurityKey"] = "functional-test-signing-key-at-least-32-chars-long",
            ["TokenOptions__AccessTokenExpirationMinutes"] = "15",
            ["TokenOptions__RefreshTokenExpirationDays"] = "7",

            // Unset URL makes Program.cs skip the Seq sink entirely.
            ["Serilog__Seq__ServerUrl"] = "",

            ["Storage__Provider"] = "Local",
            ["Storage__Local__RootPath"] = Path.Combine(Path.GetTempPath(), $"hrms-tests-{Guid.CreateVersion7():N}"),

            ["IdentityVerification__Provider"] = "Null",

            ["Seed__AdminEmail"] = AdminEmail,
            ["Seed__AdminPassword"] = AdminPassword,

            // Every test shares one host and so one rate-limit partition; the production limit of
            // 10 per 5 minutes would reject most of the suite. RateLimiterTests asserts the limiter
            // still works by configuring its own low limit.
            ["RateLimiting__Auth__PermitLimit"] = "10000"
        };

        foreach (var (key, value) in settings)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
        PublishTestConfiguration();

        // Forces the host to build, which applies migrations and seeds roles + the admin.
        using (var _ = CreateClient()) { }

        _connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],

            // Roles and the migration history survive resets: roles are reference data the seeder
            // only creates once, and wiping the history would make EF think the schema is missing.
            TablesToIgnore = ["roles", "__EFMigrationsHistory"]
        });
    }

    /// <summary>Truncates domain tables between tests, leaving schema and reference data intact.</summary>
    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connection);

        // The admin is wiped along with the other users, so re-seed it for the next test.
        using var scope = Services.CreateScope();
        await Persistence.Seeding.DatabaseSeeder.SeedAsync(Services);
    }

    public new async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}

/// <summary>Shares one container and one host across every functional test class.</summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<HrmsApiFactory>
{
    public const string Name = "hrms-api";
}
