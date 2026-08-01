using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HRMS.WebAPI.FunctionalTests;

[Collection(ApiCollection.Name)]
public class RateLimiterTests
{
    private const string PermitLimitVariable = "RateLimiting__Auth__PermitLimit";
    private const int TestPermitLimit = 3;

    private const string SuitePermitLimit = "10000";

    [Fact]
    public async Task LoginEndpoint_Should_Return429_AfterTheLimitIsExceeded()
    {
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

            statuses[0].ShouldBe(HttpStatusCode.Unauthorized);

            statuses.ShouldContain(HttpStatusCode.TooManyRequests);
        }
        finally
        {
            Environment.SetEnvironmentVariable(PermitLimitVariable, SuitePermitLimit);
        }
    }

    [Fact]
    public async Task MeEndpoint_Should_NeverReturn429_RegardlessOfHowManyTimesItIsCalled()
    {
        // /auth/me is called on every protected page load — far more often than login, register or
        // refresh — and needs a valid Bearer token just to be reached, so it carries none of the
        // credential-guessing risk the "auth" policy exists to guard against. Sharing that policy's
        // tight bucket used to mean a few minutes of normal navigation could exhaust it and read as a
        // session that expired, when nothing about the token had actually changed.
        Environment.SetEnvironmentVariable(PermitLimitVariable, TestPermitLimit.ToString());

        try
        {
            await using var factory = new ThrottledApiFactory();
            using var httpClient = factory.CreateClient();
            var client = new HrmsClient(httpClient);

            var registered = await client.RegisterJobSeekerAsync($"me-throttle-{Guid.NewGuid():N}@test.local");
            client.Authenticate(registered.DataString("accessToken"));

            for (var attempt = 0; attempt < TestPermitLimit + 5; attempt++)
            {
                (await client.GetAsync("/api/auth/me")).Status.ShouldBe(HttpStatusCode.OK);
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable(PermitLimitVariable, SuitePermitLimit);
        }
    }

    private sealed class ThrottledApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Testing");
    }
}
