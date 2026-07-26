using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Persistence
{
    public sealed class HrmsDbContextFactory : IDesignTimeDbContextFactory<HrmsDbContext>
    {
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
