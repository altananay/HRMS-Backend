using System.Net;

namespace HRMS.WebAPI.FunctionalTests;

[Collection(ApiCollection.Name)]
public class ResourceOwnershipTests : IAsyncLifetime
{
    private const string Owner = "owner@ownership.test";
    private const string Stranger = "stranger@ownership.test";
    private const string Employer = "employer@ownership.test";
    private const string Rival = "rival@ownership.test";
    private const string Password = "Passw0rd!23";

    private readonly HrmsApiFactory _factory;
    private HrmsClient _client = null!;

    public ResourceOwnershipTests(HrmsApiFactory factory) => _factory = factory;

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = new HrmsClient(_factory.CreateClient());

        (await _client.RegisterJobSeekerAsync(Owner)).IsSuccess.ShouldBeTrue();
        (await _client.RegisterJobSeekerAsync(Stranger)).IsSuccess.ShouldBeTrue();
        (await _client.RegisterEmployerAsync(Employer, "Acme")).IsSuccess.ShouldBeTrue();
        (await _client.RegisterEmployerAsync(Rival, "Rival Co")).IsSuccess.ShouldBeTrue();
        _client.Authenticate(null);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private async Task<string> CurrentUserIdAsync()
        => (await _client.GetAsync("/api/auth/me")).DataString("id");

    private async Task<(string SeekerId, string CvId)> GivenOwnerHasACvAsync()
    {
        await _client.LoginAsAsync(Owner, Password);
        var seekerId = await CurrentUserIdAsync();

        var created = await _client.PostAsync("/api/Cvs/add", new
        {
            information = "Gizli özgeçmiş.",
            skills = new[] { "csharp" }
        });
        created.Status.ShouldBe(HttpStatusCode.Created, created.Body);

        return (seekerId, created.DataString("id"));
    }

    private async Task<string> GivenEmployerHasAnAdvertisementAsync()
    {
        await _client.LoginAsAsync(Employer, Password);

        var created = await _client.PostAdvertisementAsync("Senior Backend Engineer");
        created.Status.ShouldBe(HttpStatusCode.Created, created.Body);

        return created.DataString("id");
    }

    [Fact]
    public async Task ReadingOwnCv_Should_ReturnTheOwnersDetails()
    {
        var (seekerId, _) = await GivenOwnerHasACvAsync();

        var response = await _client.GetAsync($"/api/Cvs/getbyjobseekerid/{seekerId}");

        response.Status.ShouldBe(HttpStatusCode.OK, response.Body);
        response.Data.GetProperty("email").GetString().ShouldBe(Owner);
        response.Data.GetProperty("firstName").GetString().ShouldBe("Test");
        response.Data.GetProperty("information").GetString().ShouldBe("Gizli özgeçmiş.");
    }

    [Fact]
    public async Task ReadingAnotherSeekersCv_Should_Return403()
    {
        var (seekerId, _) = await GivenOwnerHasACvAsync();

        await _client.LoginAsAsync(Stranger, Password);

        (await _client.GetAsync($"/api/Cvs/getbyjobseekerid/{seekerId}"))
            .Status.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReadingACandidatesCv_Should_BeEarnedByReceivingTheirApplication()
    {
        var advertisementId = await GivenEmployerHasAnAdvertisementAsync();
        var (seekerId, _) = await GivenOwnerHasACvAsync();

        await _client.LoginAsAsync(Employer, Password);
        (await _client.GetAsync($"/api/Cvs/getbyjobseekerid/{seekerId}"))
            .Status.ShouldBe(HttpStatusCode.Forbidden);

        await _client.LoginAsAsync(Owner, Password);
        (await _client.PostAsync("/api/JobApplications/add", new
        {
            jobAdvertisementId = advertisementId,
            jobSeekerNote = "İlgileniyorum."
        })).IsSuccess.ShouldBeTrue();

        await _client.LoginAsAsync(Employer, Password);
        (await _client.GetAsync($"/api/Cvs/getbyjobseekerid/{seekerId}"))
            .Status.ShouldBe(HttpStatusCode.OK);

        await _client.LoginAsAsync(Rival, Password);
        (await _client.GetAsync($"/api/Cvs/getbyjobseekerid/{seekerId}"))
            .Status.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeletingAnotherSeekersCv_Should_Return403_AndLeaveItIntact()
    {
        var (seekerId, cvId) = await GivenOwnerHasACvAsync();

        await _client.LoginAsAsync(Stranger, Password);
        (await _client.DeleteAsync($"/api/Cvs/deletecv/{cvId}"))
            .Status.ShouldBe(HttpStatusCode.Forbidden);

        await _client.LoginAsAsync(Owner, Password);
        (await _client.GetAsync($"/api/Cvs/getbyjobseekerid/{seekerId}"))
            .Status.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeletingOwnCv_Should_Succeed()
    {
        var (seekerId, cvId) = await GivenOwnerHasACvAsync();

        (await _client.DeleteAsync($"/api/Cvs/deletecv/{cvId}")).IsSuccess.ShouldBeTrue();

        (await _client.GetAsync($"/api/Cvs/getbyjobseekerid/{seekerId}"))
            .Status.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletingAnotherEmployersAdvertisement_Should_Return403_AndLeaveItPublished()
    {
        var advertisementId = await GivenEmployerHasAnAdvertisementAsync();

        await _client.LoginAsAsync(Rival, Password);
        (await _client.DeleteAsync($"/api/JobAdvertisements/deletebyid/{advertisementId}"))
            .Status.ShouldBe(HttpStatusCode.Forbidden);

        _client.Authenticate(null);
        (await _client.GetAsync($"/api/JobAdvertisements/getbyid/{advertisementId}"))
            .Status.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeletingOwnAdvertisement_Should_Succeed()
    {
        var advertisementId = await GivenEmployerHasAnAdvertisementAsync();

        await _client.LoginAsAsync(Employer, Password);
        (await _client.DeleteAsync($"/api/JobAdvertisements/deletebyid/{advertisementId}"))
            .IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ReadingAnApplication_Should_BeLimitedToItsTwoParties()
    {
        var advertisementId = await GivenEmployerHasAnAdvertisementAsync();

        await _client.LoginAsAsync(Owner, Password);
        var created = await _client.PostAsync("/api/JobApplications/add", new
        {
            jobAdvertisementId = advertisementId,
            jobSeekerNote = "İlgileniyorum."
        });
        created.Status.ShouldBe(HttpStatusCode.Created, created.Body);

        var applicationId = created.DataString("id");

        (await _client.GetAsync($"/api/JobApplications/getbyid/{applicationId}"))
            .Status.ShouldBe(HttpStatusCode.OK);

        await _client.LoginAsAsync(Employer, Password);
        (await _client.GetAsync($"/api/JobApplications/getbyid/{applicationId}"))
            .Status.ShouldBe(HttpStatusCode.OK);

        await _client.LoginAsAsync(Rival, Password);
        (await _client.GetAsync($"/api/JobApplications/getbyid/{applicationId}"))
            .Status.ShouldBe(HttpStatusCode.Forbidden);

        await _client.LoginAsAsync(Stranger, Password);
        (await _client.GetAsync($"/api/JobApplications/getbyid/{applicationId}"))
            .Status.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReadingAnotherCandidatesProfile_Should_Return403()
    {
        await _client.LoginAsAsync(Owner, Password);
        var seekerId = await CurrentUserIdAsync();

        (await _client.GetAsync($"/api/JobSeekers/getbyid/{seekerId}"))
            .Status.ShouldBe(HttpStatusCode.OK);

        await _client.LoginAsAsync(Stranger, Password);
        (await _client.GetAsync($"/api/JobSeekers/getbyid/{seekerId}"))
            .Status.ShouldBe(HttpStatusCode.Forbidden);

        await _client.LoginAsAsync(Rival, Password);
        (await _client.GetAsync($"/api/JobSeekers/getbyid/{seekerId}"))
            .Status.ShouldBe(HttpStatusCode.Forbidden);
    }
}
