using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HRMS.WebAPI.FunctionalTests;

/// <summary>
/// Boots the real API in memory for functional tests.
/// </summary>
/// <remarks>
/// Phase 3 extends this to start a PostgreSQL Testcontainer, point the DbContext at it, and run
/// migrations plus seed data. Right now it only overrides configuration, which is enough for the
/// routing and authorization assertions in <see cref="SecuritySmokeTests"/> — those never reach a
/// database, because authorization short-circuits before the handler runs.
/// </remarks>
public sealed class HrmsApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TokenOptions:Issuer"] = "hrms-tests",
                ["TokenOptions:Audience"] = "hrms-tests",
                ["TokenOptions:SecurityKey"] = "functional-test-signing-key-at-least-32-chars-long",
                ["TokenOptions:AccessTokenExpirationMinutes"] = "15",

                // No Seq in tests: an unset URL makes Program.cs skip the sink entirely.
                ["Serilog:Seq:ServerUrl"] = "",

                // Phase 6 replaces this with a PostgreSQL Testcontainer plus migrations and seed
                // data. The assertions in SecuritySmokeTests never reach a database — authorization
                // short-circuits before the handler runs — so a connection string that merely parses
                // is enough for them today.
                ["ConnectionStrings:Postgres"] =
                    "Host=localhost;Port=5433;Database=hrms;Username=hrms;Password=hrms",

                ["Storage:Provider"] = "Local"
            });
        });
    }
}
