using Application.Abstractions;
using Infrastructure.Services.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;
using Persistence;
using Persistence.Interceptors;
using Respawn;
using Testcontainers.PostgreSql;

namespace HRMS.Persistence.IntegrationTests;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("hrms_persistence_tests")
        .WithUsername("hrms")
        .WithPassword("hrms")
        .Build();

    private ServiceProvider _services = null!;
    private Respawner _respawner = null!;
    private NpgsqlConnection _connection = null!;

    public SettableTimeProvider Clock { get; } = new();

    public IServiceProvider Services => _services;

    public const string AdminEmail = "admin@persistence.test";
    public const string AdminPassword = "Adm!nTest12345";

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgres.GetConnectionString(),
                ["Seed:AdminEmail"] = AdminEmail,
                ["Seed:AdminPassword"] = AdminPassword
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddPersistenceServices(configuration);

        services.RemoveAll<TimeProvider>();
        services.AddSingleton<TimeProvider>(Clock);
        services.RemoveAll<AuditingSaveChangesInterceptor>();
        services.AddSingleton(provider => new AuditingSaveChangesInterceptor(
            provider.GetRequiredService<TimeProvider>()));

        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();

        _services = services.BuildServiceProvider();

        using (var scope = _services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<HrmsDbContext>().Database.MigrateAsync();
        }

        _connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public HrmsDbContext CreateContext()
    {
        var scope = _services.CreateScope();
        _scopes.Add(scope);

        return scope.ServiceProvider.GetRequiredService<HrmsDbContext>();
    }

    public T GetService<T>() where T : notnull
    {
        var scope = _services.CreateScope();
        _scopes.Add(scope);

        return scope.ServiceProvider.GetRequiredService<T>();
    }

    public async Task<T> InScopeAsync<T>(Func<IServiceProvider, Task<T>> work)
    {
        using var scope = _services.CreateScope();

        return await work(scope.ServiceProvider);
    }

    private readonly List<IServiceScope> _scopes = [];

    public Task ResetDatabaseAsync() => _respawner.ResetAsync(_connection);

    public async ValueTask DisposeAsync()
    {
        foreach (var scope in _scopes)
        {
            scope.Dispose();
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        await _services.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}

public sealed class SettableTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void Set(DateTimeOffset utcNow) => _utcNow = utcNow;

    public void Advance(TimeSpan by) => _utcNow = _utcNow.Add(by);
}

[CollectionDefinition(Name)]
public sealed class PersistenceCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "hrms-persistence";
}
