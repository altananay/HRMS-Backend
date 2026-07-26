using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.WebAPI.FunctionalTests;

[Collection(ApiCollection.Name)]
public class SecuritySmokeTests
{
    private readonly HrmsApiFactory _factory;

    public SecuritySmokeTests(HrmsApiFactory factory) => _factory = factory;

    // Every anonymous endpoint must be listed here. A new one that is not fails the suite on purpose,
    // which is what turns "this is deliberately public" into a decision somebody signed off on.
    private static readonly string[] PublicEndpoints =
    [
        "api/Contacts",

        "api/JobAdvertisements/getall",
        "api/JobAdvertisements/getbyid/{id:guid}",

        "api/JobPosition/getall",
        "api/JobPosition/getbyid/{id:guid}",

        "api/Employers/getbyemployerid/{id:guid}",

        "api/auth/login",
        "api/auth/register/jobseeker",
        "api/auth/register/employer",
        "api/auth/refresh",

        "api/auth/logout"
    ];

    private static readonly string[] MustRequireAuthentication =
    [
        "/api/JobSeekers/getall",
        "/api/Employers/getall",
        "/api/Users/getall",
        "/api/Cvs/getall",
        "/api/SystemStaffs/getall",
        "/api/Contacts",
        "/api/auth/me"
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

        foreach (var route in MustRequireAuthentication)
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

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
