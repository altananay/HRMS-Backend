using System.Net;

namespace HRMS.WebAPI.FunctionalTests;

/// <summary>
/// Access to a single record by its id, for callers who have no claim to it.
/// </summary>
/// <remarks>
/// An audit found five holes of exactly one shape. The migration swept the write paths (the owning
/// id comes from the token) and the list paths (results are narrowed by role and token), but not the
/// by-id paths — and holding a role is not the same as owning a row. Two of the five were
/// destructive and were demonstrated against a running instance: an employer deleted a rival's
/// advertisement, and a job seeker permanently deleted a stranger's CV, cascading to their
/// education, experience, language, project and file rows.
///
/// The pattern to keep in mind when adding an endpoint: <c>GetAll</c> being scoped says nothing
/// about <c>GetById</c>, and a guard on <c>Update</c> says nothing about <c>Delete</c>. Both of
/// those asymmetries shipped here.
/// </remarks>
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

    // -------------------------------------------------------------------------------------------
    // Fixtures
    // -------------------------------------------------------------------------------------------

    private async Task<string> CurrentUserIdAsync()
        => (await _client.GetAsync("/api/auth/me")).DataString("id");

    /// <summary>Signs in as the owner, gives them a CV, and returns their id and the CV's id.</summary>
    private async Task<(string SeekerId, string CvId)> GivenOwnerHasACvAsync()
    {
        await _client.LoginAsAsync(Owner, Password);
        var seekerId = await CurrentUserIdAsync();

        (await _client.PostAsync("/api/Cvs/add", new
        {
            information = "Gizli özgeçmiş.",
            skills = new[] { "csharp" }
        })).IsSuccess.ShouldBeTrue();

        var cv = await _client.GetAsync($"/api/Cvs/getbyjobseekerid/{seekerId}");
        cv.Status.ShouldBe(HttpStatusCode.OK, cv.Body);

        return (seekerId, cv.DataString("id"));
    }

    /// <summary>Signs in as the employer, publishes an advertisement, and returns its id.</summary>
    private async Task<string> GivenEmployerHasAnAdvertisementAsync()
    {
        await _client.LoginAsAsync(Employer, Password);
        (await _client.PostAdvertisementAsync("Senior Backend Engineer")).IsSuccess.ShouldBeTrue();

        _client.Authenticate(null);
        var board = await _client.GetAsync("/api/JobAdvertisements/getall");

        return board.Data.GetProperty("items")[0].GetProperty("id").GetString()!;
    }

    // -------------------------------------------------------------------------------------------
    // Reading a CV — the owner, an employer who received an application, or an admin
    // -------------------------------------------------------------------------------------------

    /// <summary>
    /// Regression guard for a NullReferenceException, not an authorization rule: every CV read
    /// answered 500, for the owner too, because the repository never included the JobSeeker
    /// navigation that the DTO projection reads the name and email from. No test read a CV back, so
    /// nothing caught it — this one asserts the payload, not just the status.
    /// </summary>
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

    /// <summary>
    /// The employer clause is a data question, not a role check: the same employer is refused before
    /// the application exists and admitted after it, with no change of role in between.
    /// </summary>
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

        // A different employer, holding the identical role, still gets nothing.
        await _client.LoginAsAsync(Rival, Password);
        (await _client.GetAsync($"/api/Cvs/getbyjobseekerid/{seekerId}"))
            .Status.ShouldBe(HttpStatusCode.Forbidden);
    }

    // -------------------------------------------------------------------------------------------
    // Destructive paths
    // -------------------------------------------------------------------------------------------

    /// <summary>
    /// Demonstrated against a running instance before the fix: this returned 200 and the CV was gone
    /// from the table. There is no deleted_at column on cvs, so the loss is permanent and cascades.
    /// </summary>
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

    /// <summary>
    /// The asymmetry that made this reachable: Update called the ownership guard and Delete did not,
    /// even though the guard's own comment described deletion as part of what it was written for.
    /// The update half was covered by a test; this half was not.
    /// </summary>
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

    // -------------------------------------------------------------------------------------------
    // Reading a single application, and a single profile
    // -------------------------------------------------------------------------------------------

    /// <summary>
    /// An application carries the applicant's name, their note and the employer's private note, so
    /// only the two parties to it may read it. GetAll was scoped from the token; this was not.
    /// </summary>
    [Fact]
    public async Task ReadingAnApplication_Should_BeLimitedToItsTwoParties()
    {
        var advertisementId = await GivenEmployerHasAnAdvertisementAsync();

        await _client.LoginAsAsync(Owner, Password);
        (await _client.PostAsync("/api/JobApplications/add", new
        {
            jobAdvertisementId = advertisementId,
            jobSeekerNote = "İlgileniyorum."
        })).IsSuccess.ShouldBeTrue();

        var applicationId = (await _client.GetAsync("/api/JobApplications/getall"))
            .Data.GetProperty("items")[0].GetProperty("id").GetString()!;

        // The applicant reads their own.
        (await _client.GetAsync($"/api/JobApplications/getbyid/{applicationId}"))
            .Status.ShouldBe(HttpStatusCode.OK);

        // So does the employer whose advertisement it is.
        await _client.LoginAsAsync(Employer, Password);
        (await _client.GetAsync($"/api/JobApplications/getbyid/{applicationId}"))
            .Status.ShouldBe(HttpStatusCode.OK);

        // Nobody else, whichever role they hold.
        await _client.LoginAsAsync(Rival, Password);
        (await _client.GetAsync($"/api/JobApplications/getbyid/{applicationId}"))
            .Status.ShouldBe(HttpStatusCode.Forbidden);

        await _client.LoginAsAsync(Stranger, Password);
        (await _client.GetAsync($"/api/JobApplications/getbyid/{applicationId}"))
            .Status.ShouldBe(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Without this the id space is a directory: every candidate's email and date of birth, readable
    /// by anyone with an account.
    /// </summary>
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
