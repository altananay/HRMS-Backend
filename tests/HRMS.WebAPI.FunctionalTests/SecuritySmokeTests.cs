using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.WebAPI.FunctionalTests;

/// <summary>
/// Guards the single worst property of the pre-migration API: essentially every endpoint was
/// anonymous.
/// </summary>
/// <remarks>
/// There was not one <c>[Authorize]</c> attribute in the entire solution. Authorization lived only
/// in <c>[SecuredOperation]</c> aspects on manager methods, and those were commented out on most
/// write paths — so <c>GET /api/JobSeekers/getall</c> and <c>GET /api/Employers/getall</c> returned
/// every user record, <b>PasswordHash and PasswordSalt included</b>, to an unauthenticated caller.
///
/// These tests enumerate the real endpoint table rather than a hand-written list, so an endpoint
/// added later is covered the moment it exists. A new endpoint is protected by the authorization
/// fallback policy by default; opening it requires adding it to <see cref="PublicEndpoints"/> here,
/// which makes "this is deliberately public" a reviewable decision instead of an oversight.
/// </remarks>
public class SecuritySmokeTests : IClassFixture<HrmsApiFactory>
{
    private readonly HrmsApiFactory _factory;

    public SecuritySmokeTests(HrmsApiFactory factory) => _factory = factory;

    /// <summary>Routes that are intentionally reachable without a token.</summary>
    private static readonly string[] PublicEndpoints =
    [
        // Public contact form.
        "api/Contacts",

        // The job board itself. Browsing listings must not require an account — this is the
        // product's entire front page.
        "api/JobAdvertisements/getall",
        "api/JobAdvertisements/getbyid/{id:guid}",

        // Position lookup, needed to render the board's filters.
        "api/JobPosition/getall",
        "api/JobPosition/getbyid/{id:guid}",

        // Company profile behind a listing. Returns EmployerDetailDto, which carries no password
        // material and no national ID.
        "api/Employers/getbyemployerid/{id:guid}"

        // The auth endpoints are absent because the three legacy auth controllers were deleted in
        // Phase 3; Phase 4 adds a single AuthController and its routes get reviewed onto this list
        // then. Until then the API has no way to authenticate, which this suite asserts by
        // expecting 401 rather than by skipping.
    ];

    private IReadOnlyList<(string Route, bool AllowsAnonymous)> GetEndpoints()
    {
        using var scope = _factory.Services.CreateScope();
        var source = scope.ServiceProvider.GetRequiredService<EndpointDataSource>();

        return source.Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is not null)
            .Select(endpoint => (
                Route: endpoint.RoutePattern.RawText ?? string.Empty,
                AllowsAnonymous: endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null))
            .ToList();
    }

    [Fact]
    public void EndpointTable_Should_NotBeEmpty()
    {
        // If this fails the other tests are vacuously passing.
        GetEndpoints().ShouldNotBeEmpty();
    }

    [Fact]
    public void EveryAnonymousEndpoint_Should_BeOnThePublicAllowList()
    {
        var unexpected = GetEndpoints()
            .Where(endpoint => endpoint.AllowsAnonymous)
            .Select(endpoint => endpoint.Route)
            .Where(route => !PublicEndpoints.Contains(route, StringComparer.OrdinalIgnoreCase))
            .Distinct()
            .ToList();

        unexpected.ShouldBeEmpty(
            "These endpoints are anonymous but not on the reviewed public allow-list: " +
            string.Join(", ", unexpected));
    }

    [Fact]
    public async Task ProtectedEndpoints_Should_Return401_When_CalledAnonymously()
    {
        using var client = _factory.CreateClient();

        // The exact endpoints that leaked password hashes to anonymous callers before the migration.
        string[] mustBeProtected =
        [
            "/api/JobSeekers/getall",
            "/api/Employers/getall",
            "/api/Users/getall",
            "/api/Cvs/getall",
            "/api/SystemStaffs/getall",
            "/api/Logs/errors",
            "/api/Logs/infos"
        ];

        foreach (var route in mustBeProtected)
        {
            var response = await client.GetAsync(route, TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized, $"{route} must require authentication");
        }
    }

    [Fact]
    public async Task PublicJobBoard_Should_BeBrowsableAnonymously()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/JobAdvertisements/getall", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldNotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedWriteEndpoints_Should_Return401_When_CalledAnonymously()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync(
            "/api/JobAdvertisements/add",
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"),
            TestContext.Current.CancellationToken);

        // Previously anonymous: JobAdvertisementManager.Add had its
        // //[SecuredOperation("employer")] commented out.
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
