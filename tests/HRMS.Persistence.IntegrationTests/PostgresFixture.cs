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

/// <summary>
/// One PostgreSQL container for the whole assembly, with the real migrations applied to it.
/// </summary>
/// <remarks>
/// The API is not involved: these tests talk to <see cref="HrmsDbContext"/> directly, because what
/// is under test is the mapping — indexes, foreign keys, delete rules, query filters, the auditing
/// interceptor — rather than any HTTP behaviour. Going through the API would exercise the same
/// database but attribute every failure to a controller.
///
/// <see cref="Clock"/> is settable so the auditing assertions can name an exact instant instead of
/// comparing against <c>DateTime.UtcNow</c> with a tolerance.
/// </remarks>
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

    /// <summary>The clock the auditing interceptor reads. Tests set it before saving.</summary>
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

        // AddPersistenceServices registers TimeProvider.System; replace it so the interceptor reads
        // a clock the tests control. Same for the interceptor itself, which captured the old one.
        services.RemoveAll<TimeProvider>();
        services.AddSingleton<TimeProvider>(Clock);
        services.RemoveAll<AuditingSaveChangesInterceptor>();
        services.AddSingleton(provider => new AuditingSaveChangesInterceptor(
            provider.GetRequiredService<TimeProvider>()));

        // Only the one Infrastructure service the seeder needs. AddInfrastructureServices would drag
        // in storage and identity-verification options that have nothing to do with persistence.
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();

        _services = services.BuildServiceProvider();

        // The real migrations, against an empty database — which is itself the first assertion.
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

    /// <summary>
    /// A context with its own change tracker.
    /// </summary>
    /// <remarks>
    /// Assertions take a fresh one so they read the database rather than the tracker's copy of the
    /// entity that was just written — otherwise a value the interceptor never persisted would still
    /// appear to be there.
    /// </remarks>
    public HrmsDbContext CreateContext()
    {
        var scope = _services.CreateScope();
        _scopes.Add(scope);

        return scope.ServiceProvider.GetRequiredService<HrmsDbContext>();
    }

    /// <summary>Resolves a service in its own scope. For reads; anything that writes needs
    /// <see cref="InScopeAsync{T}"/> so the repository and the context it saves through are the same
    /// instance.</summary>
    public T GetService<T>() where T : notnull
    {
        var scope = _services.CreateScope();
        _scopes.Add(scope);

        return scope.ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Runs work inside one scope, so every scoped service it resolves shares a DbContext.
    /// </summary>
    /// <remarks>
    /// A repository resolved separately from the context it is meant to save through gets its own
    /// DbContext, and <c>SaveChanges</c> on the other one silently persists nothing — which looks
    /// exactly like a broken repository.
    /// </remarks>
    public async Task<T> InScopeAsync<T>(Func<IServiceProvider, Task<T>> work)
    {
        using var scope = _services.CreateScope();

        return await work(scope.ServiceProvider);
    }

    private readonly List<IServiceScope> _scopes = [];

    /// <summary>Truncates every table, roles included — seeding is one of the things under test.</summary>
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

/// <summary>A clock the tests move by hand.</summary>
/// <remarks>
/// Hand-written rather than pulling in Microsoft.Extensions.TimeProvider.Testing: two members are
/// enough, and the auditing interceptor only ever calls <see cref="GetUtcNow"/>.
/// </remarks>
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
