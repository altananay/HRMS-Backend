using Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace HRMS.WebAPI.FunctionalTests;

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

    public const string AdminEmail = "admin@hrms.test";

    public const string AdminPassword = "Adm!nTest12345";

    public RecordingEmailSender Mail { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Mail);
        });
    }

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

            ["Serilog__Seq__ServerUrl"] = "",

            ["Storage__Provider"] = "Local",
            ["Storage__Local__RootPath"] = Path.Combine(Path.GetTempPath(), $"hrms-tests-{Guid.CreateVersion7():N}"),

            ["IdentityVerification__Provider"] = "Null",

            ["Email__Host"] = "",
            ["PasswordReset__LinkBaseUrl"] = "http://localhost:3000/reset-password",
            ["PasswordReset__TokenLifetimeMinutes"] = "60",

            ["Seed__AdminEmail"] = AdminEmail,
            ["Seed__AdminPassword"] = AdminPassword,

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

        using (var _ = CreateClient()) { }

        _connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],

            TablesToIgnore = ["roles", "__EFMigrationsHistory"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connection);

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

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<HrmsApiFactory>
{
    public const string Name = "hrms-api";
}
