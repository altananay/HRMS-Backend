using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HRMS.WebAPI.FunctionalTests;

/// <summary>
/// The auth endpoints throttle repeated attempts.
/// </summary>
/// <remarks>
/// Runs on its own host with a deliberately tiny limit, because the shared factory raises the limit
/// so the rest of the suite can run at all. Without this test the limiter would be configured but
/// never proven — the same "present but unverified" trap as the security-stamp claim that was
/// written into every token and read by nothing.
///
/// Throttling is the only real defence here against credential stuffing: the passwords are properly
/// hashed, but nothing else stops an attacker working through a list of them.
///
/// Joined to the shared collection deliberately. The limit is configured through a process-wide
/// environment variable, so running in parallel with the scenario tests would throttle them; xUnit
/// serialises tests within a collection.
/// </remarks>
[Collection(ApiCollection.Name)]
public class RateLimiterTests
{
    private const string PermitLimitVariable = "RateLimiting__Auth__PermitLimit";
    private const int TestPermitLimit = 3;

    /// <summary>Restored afterwards so the shared host's raised limit is not left throttled.</summary>
    private const string SuitePermitLimit = "10000";

    [Fact]
    public async Task LoginEndpoint_Should_Return429_AfterTheLimitIsExceeded()
    {
        // Set before the host is built: under minimal hosting Program.cs reads configuration while
        // constructing services, which is earlier than any ConfigureWebHost delegate runs.
        Environment.SetEnvironmentVariable(PermitLimitVariable, TestPermitLimit.ToString());

        try
        {
            await using var factory = new ThrottledApiFactory();
            using var client = factory.CreateClient();

            var statuses = new List<HttpStatusCode>();

            for (var attempt = 0; attempt < TestPermitLimit + 2; attempt++)
            {
                var response = await client.PostAsJsonAsync(
                    "/api/auth/login",
                    new { email = "nobody@test.local", password = "WrongPassword!1" },
                    TestContext.Current.CancellationToken);

                statuses.Add(response.StatusCode);
            }

            // The first attempts are genuine credential rejections — if they were already throttled
            // the limit would be too low for normal use.
            statuses[0].ShouldBe(HttpStatusCode.Unauthorized);

            // And the attempts past the limit are rejected before reaching the handler.
            statuses.ShouldContain(HttpStatusCode.TooManyRequests);
        }
        finally
        {
            Environment.SetEnvironmentVariable(PermitLimitVariable, SuitePermitLimit);
        }
    }

    /// <summary>
    /// A host identical to the shared one apart from the auth rate limit.
    /// </summary>
    /// <remarks>
    /// Reuses the connection string and other settings the shared factory already published as
    /// environment variables, so it needs no container of its own. The requests under test never
    /// reach the database anyway: they are either throttled or fail on credentials.
    /// </remarks>
    private sealed class ThrottledApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Testing");
    }
}
