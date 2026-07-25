using System.Net;

namespace HRMS.WebAPI.FunctionalTests;

/// <summary>
/// The product's main path, plus the authorization boundaries around it.
/// </summary>
/// <remarks>
/// Every step here exercised something that was broken before the migration: CV update returned 500
/// unconditionally, editing an advertisement silently deactivated it, nothing stopped duplicate
/// applications, and one employer could read another's data because the owning id arrived in the
/// request body rather than from the token.
/// </remarks>
[Collection(ApiCollection.Name)]
public class JobLifecycleScenarioTests : IAsyncLifetime
{
    private const string SeekerEmail = "seeker@lifecycle.test";
    private const string EmployerEmail = "employer@lifecycle.test";
    private const string RivalEmail = "rival@lifecycle.test";
    private const string Password = "Passw0rd!23";

    private readonly HrmsApiFactory _factory;
    private HrmsClient _client = null!;

    public JobLifecycleScenarioTests(HrmsApiFactory factory) => _factory = factory;

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = new HrmsClient(_factory.CreateClient());
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task HiringFlow_Should_WorkEndToEnd_AndEnforceItsBoundaries()
    {
        // 1-2. Both actor types register through one unified auth surface.
        (await _client.RegisterEmployerAsync(EmployerEmail, "Acme")).IsSuccess.ShouldBeTrue();
        (await _client.RegisterJobSeekerAsync(SeekerEmail)).IsSuccess.ShouldBeTrue();
        (await _client.RegisterEmployerAsync(RivalEmail, "Rival Co")).IsSuccess.ShouldBeTrue();
        _client.Authenticate(null);

        // 3. Employer publishes an advertisement. EmployerId comes from the token, never the body.
        await _client.LoginAsAsync(EmployerEmail, Password);
        var posted = await _client.PostAdvertisementAsync("Senior Backend Engineer");
        posted.IsSuccess.ShouldBeTrue(posted.Body);

        // 4. The board is browsable without an account.
        _client.Authenticate(null);
        var board = await _client.GetAsync("/api/JobAdvertisements/getall");
        board.Status.ShouldBe(HttpStatusCode.OK);

        var items = board.Data.GetProperty("items");
        items.GetArrayLength().ShouldBe(1);

        var advertisementId = items[0].GetProperty("id").GetString()!;

        // The denormalized company columns are gone, but the contract still carries them —
        // projected from the Employer navigation.
        items[0].GetProperty("companyName").GetString().ShouldBe("Acme");
        items[0].GetProperty("jobPositionName").GetString().ShouldBe("Backend Developer");
        items[0].GetProperty("isActive").GetBoolean().ShouldBeTrue();

        // 5. Editing must not silently deactivate the advertisement, which is what the old
        //    Update did by never copying Status onto the replacement document.
        await _client.LoginAsAsync(EmployerEmail, Password);
        var edited = await _client.PutAsync("/api/JobAdvertisements/update", new
        {
            id = advertisementId,
            title = "Senior Backend Engineer (updated)",
            jobPositionName = "Backend Developer",
            description = "Bu ilan açıklaması doğrulamadan geçecek kadar uzun olmalıdır.",
            openPositions = 3,
            jobType = "FullTime",
            deadline = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)),
            skills = new[] { "csharp", "postgresql" },
            isActive = true
        });
        edited.IsSuccess.ShouldBeTrue(edited.Body);

        _client.Authenticate(null);
        var afterEdit = await _client.GetAsync($"/api/JobAdvertisements/getbyid/{advertisementId}");
        afterEdit.Data.GetProperty("isActive").GetBoolean().ShouldBeTrue();

        // 6. A rival employer cannot edit somebody else's listing. Holding the employer role is not
        //    ownership — the old code took EmployerId from the request body, so anyone could.
        await _client.LoginAsAsync(RivalEmail, Password);
        var hijack = await _client.PutAsync("/api/JobAdvertisements/update", new
        {
            id = advertisementId,
            title = "Hijacked",
            jobPositionName = "Backend Developer",
            description = "Bu ilan açıklaması doğrulamadan geçecek kadar uzun olmalıdır.",
            openPositions = 1,
            jobType = "FullTime",
            deadline = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)),
            skills = new[] { "csharp" },
            isActive = true
        });
        hijack.Status.ShouldBe(HttpStatusCode.Forbidden);

        // 7-8. Seeker creates a CV, then updates it. Update used to throw unconditionally because
        //      its guard threw when a CV existed — on an operation that requires one.
        await _client.LoginAsAsync(SeekerEmail, Password);
        (await _client.PostAsync("/api/Cvs/add", new
        {
            information = "Sekiz yıllık backend deneyimi.",
            skills = new[] { "csharp", "postgresql" }
        })).IsSuccess.ShouldBeTrue();

        var cvUpdate = await _client.PutAsync("/api/Cvs/update", new
        {
            information = "Dokuz yıllık backend deneyimi.",
            skills = new[] { "csharp", "postgresql", "docker" }
        });
        cvUpdate.IsSuccess.ShouldBeTrue(cvUpdate.Body);

        // 9. Application succeeds once.
        var applied = await _client.PostAsync("/api/JobApplications/add", new
        {
            jobAdvertisementId = advertisementId,
            jobSeekerNote = "İlgileniyorum."
        });
        applied.IsSuccess.ShouldBeTrue(applied.Body);

        // 10. And exactly once — the unique (seeker, advertisement) index, which nothing enforced.
        var duplicate = await _client.PostAsync("/api/JobApplications/add", new
        {
            jobAdvertisementId = advertisementId,
            jobSeekerNote = "Tekrar."
        });
        duplicate.Status.ShouldBe(HttpStatusCode.Conflict);

        // 11. The employer sees the application; results are scoped from the token, not a route id.
        await _client.LoginAsAsync(EmployerEmail, Password);
        var employerView = await _client.GetAsync("/api/JobApplications/getall");
        employerView.Status.ShouldBe(HttpStatusCode.OK);
        employerView.Data.GetProperty("items").GetArrayLength().ShouldBe(1);

        var applicationId = employerView.Data.GetProperty("items")[0].GetProperty("id").GetString()!;
        employerView.Data.GetProperty("items")[0].GetProperty("status").GetString().ShouldBe("Submitted");

        // 12. The rival employer sees nothing, despite holding the same role.
        await _client.LoginAsAsync(RivalEmail, Password);
        (await _client.GetAsync("/api/JobApplications/getall"))
            .Data.GetProperty("items").GetArrayLength().ShouldBe(0);

        // 13. Moderation moves the application through a typed status, not a free-text string.
        await _client.LoginAsAsync(EmployerEmail, Password);
        var moderated = await _client.PutAsync("/api/JobApplications/update", new
        {
            id = applicationId,
            status = "Accepted",
            employerNote = "Mülakata davet edildi."
        });
        moderated.IsSuccess.ShouldBeTrue(moderated.Body);

        // 14. The seeker sees the new status on their own application.
        await _client.LoginAsAsync(SeekerEmail, Password);
        var seekerView = await _client.GetAsync("/api/JobApplications/getall");
        seekerView.Data.GetProperty("items")[0].GetProperty("status").GetString().ShouldBe("Accepted");

        // 15. Admin-only surfaces stay closed to a job seeker, and leak nothing when open.
        (await _client.GetAsync("/api/JobSeekers/getall")).Status.ShouldBe(HttpStatusCode.Forbidden);

        await _client.LoginAsAsync(HrmsApiFactory.AdminEmail, HrmsApiFactory.AdminPassword);
        var adminView = await _client.GetAsync("/api/JobSeekers/getall");
        adminView.Status.ShouldBe(HttpStatusCode.OK);

        foreach (var forbidden in new[] { "passwordHash", "passwordSalt", "securityStamp", "nationalId" })
        {
            adminView.Body.ShouldNotContain(forbidden, Case.Insensitive);
        }
    }

    /// <summary>
    /// Deleting a shared job position that advertisements still reference is a conflict, not a crash.
    /// </summary>
    /// <remarks>
    /// Positions used to be created one-per-advertisement and deleted alongside them. Now the name is
    /// unique and reused, so the foreign key is RESTRICT and the endpoint has to say why.
    /// </remarks>
    [Fact]
    public async Task DeletingAJobPositionInUse_Should_Return409()
    {
        await _client.RegisterEmployerAsync(EmployerEmail, "Acme");
        await _client.LoginAsAsync(EmployerEmail, Password);
        (await _client.PostAdvertisementAsync("Backend Engineer")).IsSuccess.ShouldBeTrue();

        _client.Authenticate(null);
        var positions = await _client.GetAsync("/api/JobPosition/getall");
        var positionId = positions.Data.GetProperty("items")[0].GetProperty("id").GetString()!;

        await _client.LoginAsAsync(HrmsApiFactory.AdminEmail, HrmsApiFactory.AdminPassword);

        (await _client.DeleteAsync($"/api/JobPosition/deletebyid/{positionId}")).Status
            .ShouldBe(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Two advertisements naming the same position share one row rather than creating two.
    /// </summary>
    [Fact]
    public async Task PublishingTwoAdvertisements_Should_ReuseTheSameJobPosition()
    {
        await _client.RegisterEmployerAsync(EmployerEmail, "Acme");
        await _client.LoginAsAsync(EmployerEmail, Password);

        await _client.PostAdvertisementAsync("Backend Engineer I", "Backend Developer");
        await _client.PostAdvertisementAsync("Backend Engineer II", "Backend Developer");

        _client.Authenticate(null);
        var positions = await _client.GetAsync("/api/JobPosition/getall");

        positions.Data.GetProperty("items").GetArrayLength().ShouldBe(1);
    }
}
