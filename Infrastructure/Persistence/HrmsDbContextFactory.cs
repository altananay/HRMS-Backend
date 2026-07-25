using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Persistence
{
    /// <summary>
    /// Design-time factory used by <c>dotnet ef migrations add</c> / <c>database update</c>.
    /// </summary>
    /// <remarks>
    /// Without this, the EF tools have to boot the whole WebAPI host — including JWT configuration
    /// validation and the Serilog sinks — just to read the model. Keeping a dedicated factory means
    /// scaffolding a migration needs nothing but a connection string.
    ///
    /// The value is only ever used to build the model, never to connect during scaffolding, so the
    /// local default is fine; override with ConnectionStrings__Postgres when pointing at another
    /// database.
    /// </remarks>
    public sealed class HrmsDbContextFactory : IDesignTimeDbContextFactory<HrmsDbContext>
    {
        // Port 5433 matches docker-compose.yml, which deliberately avoids 5432 so it cannot collide
        // with a natively-installed PostgreSQL service.
        private const string DefaultConnectionString =
            "Host=localhost;Port=5433;Database=hrms;Username=hrms;Password=hrms";

        public HrmsDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__Postgres") ?? DefaultConnectionString;

            var options = new DbContextOptionsBuilder<HrmsDbContext>()
                .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(HrmsDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention()
                .Options;

            return new HrmsDbContext(options);
        }
    }
}
