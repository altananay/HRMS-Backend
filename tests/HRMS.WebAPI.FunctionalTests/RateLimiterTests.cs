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

    private sealed class ThrottledApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Testing");
    }
}
