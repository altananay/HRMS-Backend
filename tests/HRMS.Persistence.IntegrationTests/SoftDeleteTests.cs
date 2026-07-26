namespace HRMS.Persistence.IntegrationTests;

/// <summary>
/// Soft delete, and the query filters that make it mean anything.
/// </summary>
/// <remarks>
/// Selective on purpose: only <c>users</c> and <c>job_advertisements</c> carry <c>deleted_at</c>.
/// A closed account has to keep its applications and hiring history intact, and a withdrawn
/// advertisement is still the thing past applications point at. Contacts, CVs and job positions
/// carry no history worth preserving and are deleted outright.
///
/// The filters matter as much as the column. Without one on every dependent, a soft-deleted seeker
/// would vanish from queries while their CV, educations and applications stayed visible — worse
/// than no soft delete at all, because the data looks orphaned rather than hidden.
/// </remarks>
[Collection(PersistenceCollection.Name)]
public class SoftDeleteTests(PostgresFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await fixture.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RemovingAUser_Should_BecomeAnUpdateRatherThanADelete()
    {
        Guid seekerId;

        await using (var arrange = fixture.CreateContext())
        {
            var seeker = Given.JobSeeker("soft@test.local");
            arrange.Add(seeker);
            await arrange.SaveChangesAsync();

            arrange.Remove(seeker);
            await arrange.SaveChangesAsync();
            seekerId = seeker.Id;
        }

        await using var assert = fixture.CreateContext();

        // Gone from normal queries...
        (await assert.JobSeekers.CountAsync(seeker => seeker.Id == seekerId)).ShouldBe(0);

        // ...but the row is still there, stamped.
        var row = await assert.JobSeekers.IgnoreQueryFilters().SingleAsync(seeker => seeker.Id == seekerId);
        row.DeletedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task RemovingAnAdvertisement_Should_HideItWithoutLosingItsApplications()
    {
        Guid advertisementId;

        await using (var arrange = fixture.CreateContext())
        {
            var employer = Given.Employer("soft-employer@test.local");
            var seeker = Given.JobSeeker("soft-applicant@test.local");
            var position = Given.JobPosition("Soft Delete Position");
            arrange.AddRange(employer, seeker, position);

            var advertisement = Given.JobAdvertisement(employer.Id, position.Id);
            arrange.Add(advertisement);
            arrange.Add(Given.JobApplication(advertisement.Id, seeker.Id));
            await arrange.SaveChangesAsync();

            advertisementId = advertisement.Id;
        }

        // A fresh context, loading only the advertisement — the shape the managers actually use,
        // which is what makes the delete soft all the way down. See
        // SoftDelete_Should_StillCascadeToDependentsLoadedIntoTheSameContext for why that matters.
        await using (var act = fixture.CreateContext())
        {
            act.Remove(await act.JobAdvertisements.SingleAsync(ad => ad.Id == advertisementId));
            await act.SaveChangesAsync();
        }

        await using var assert = fixture.CreateContext();

        (await assert.JobAdvertisements.CountAsync(ad => ad.Id == advertisementId)).ShouldBe(0);

        // The application row survives — that is the whole reason this delete is soft.
        (await assert.JobApplications.IgnoreQueryFilters()
            .CountAsync(application => application.JobAdvertisementId == advertisementId)).ShouldBe(1);
    }

    /// <summary>
    /// The filter that had to be repeated onto every dependent: a CV whose owner is soft-deleted must
    /// disappear with them, not linger as a record with no reachable owner.
    /// </summary>
    [Fact]
    public async Task SoftDeletingASeeker_Should_HideTheirCvAndItsChildren()
    {
        Guid seekerId, cvId;

        await using (var arrange = fixture.CreateContext())
        {
            var seeker = Given.JobSeeker("hidden@test.local");
            arrange.Add(seeker);

            var cv = Given.Cv(seeker.Id);
            arrange.Add(cv);
            await arrange.SaveChangesAsync();

            seekerId = seeker.Id;
            cvId = cv.Id;
        }

        await using (var act = fixture.CreateContext())
        {
            act.Remove(await act.JobSeekers.SingleAsync(seeker => seeker.Id == seekerId));
            await act.SaveChangesAsync();
        }

        await using var assert = fixture.CreateContext();

        (await assert.Cvs.CountAsync(cv => cv.Id == cvId)).ShouldBe(0);
        (await assert.Educations.CountAsync(row => row.CvId == cvId)).ShouldBe(0);
        (await assert.JobExperiences.CountAsync(row => row.CvId == cvId)).ShouldBe(0);
        (await assert.CvLanguages.CountAsync(row => row.CvId == cvId)).ShouldBe(0);
        (await assert.CvProjects.CountAsync(row => row.CvId == cvId)).ShouldBe(0);
        (await assert.CvFiles.CountAsync(row => row.CvId == cvId)).ShouldBe(0);

        // Hidden, not destroyed: reinstating the owner would bring all of it back.
        (await assert.Cvs.IgnoreQueryFilters().CountAsync(cv => cv.Id == cvId)).ShouldBe(1);
        (await assert.Educations.IgnoreQueryFilters().CountAsync(row => row.CvId == cvId)).ShouldBe(1);
    }

    /// <summary>
    /// The sharp edge of mixing soft delete with cascading foreign keys, pinned so it is a known
    /// property rather than a surprise.
    /// </summary>
    /// <remarks>
    /// The interceptor rewrites the delete of an <c>ISoftDeletable</c> into an update, but it only
    /// sees that one entry. Dependents that EF has already marked <c>Deleted</c> by cascading through
    /// the change tracker are not soft-deletable and stay deleted — so the owner is merely hidden
    /// while their CV is destroyed outright.
    ///
    /// It does not bite today because every manager loads the principal alone: JobSeekerManager and
    /// EmployerManager both go through <c>Ensure…ExistsAsync</c>, which does not Include anything, so
    /// there are no tracked dependents to cascade to and the database-level cascade never fires
    /// either — the statement EF sends is an UPDATE. Adding an Include to one of those load paths
    /// would silently turn a hide into a destroy, and this test is what would catch it.
    /// </remarks>
    [Fact]
    public async Task SoftDelete_Should_StillCascadeToDependentsLoadedIntoTheSameContext()
    {
        Guid seekerId, cvId;

        await using var context = fixture.CreateContext();

        var seeker = Given.JobSeeker("tracked-cascade@test.local");
        context.Add(seeker);

        var cv = Given.Cv(seeker.Id);
        context.Add(cv);
        await context.SaveChangesAsync();

        seekerId = seeker.Id;
        cvId = cv.Id;

        // Both are tracked here, unlike in the manager path.
        context.Remove(seeker);
        await context.SaveChangesAsync();

        await using var assert = fixture.CreateContext();

        // The owner is hidden...
        assert.JobSeekers.IgnoreQueryFilters().Single(row => row.Id == seekerId).DeletedAt.ShouldNotBeNull();

        // ...and the CV is gone for good, which is the asymmetry to be aware of.
        (await assert.Cvs.IgnoreQueryFilters().CountAsync(row => row.Id == cvId)).ShouldBe(0);
    }

    [Fact]
    public async Task SoftDeletingAUser_Should_HideTheirRefreshTokens()
    {
        Guid seekerId;

        await using (var arrange = fixture.CreateContext())
        {
            var seeker = Given.JobSeeker("token-hidden@test.local");
            arrange.Add(seeker);
            arrange.Add(new Domain.Entities.RefreshToken
            {
                UserId = seeker.Id,
                TokenHash = "hidden-token",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
            await arrange.SaveChangesAsync();
            seekerId = seeker.Id;
        }

        await using (var act = fixture.CreateContext())
        {
            act.Remove(await act.JobSeekers.SingleAsync(seeker => seeker.Id == seekerId));
            await act.SaveChangesAsync();
        }

        await using var assert = fixture.CreateContext();

        (await assert.RefreshTokens.CountAsync(token => token.UserId == seekerId)).ShouldBe(0);
        (await assert.RefreshTokens.IgnoreQueryFilters().CountAsync(token => token.UserId == seekerId)).ShouldBe(1);
    }

    /// <summary>Contacts carry no history, so their delete is a real one.</summary>
    [Fact]
    public async Task RemovingAContact_Should_DeleteTheRow()
    {
        Guid contactId;

        await using (var arrange = fixture.CreateContext())
        {
            var contact = Given.Contact();
            arrange.Add(contact);
            await arrange.SaveChangesAsync();

            arrange.Remove(contact);
            await arrange.SaveChangesAsync();
            contactId = contact.Id;
        }

        await using var assert = fixture.CreateContext();

        (await assert.Contacts.IgnoreQueryFilters().CountAsync(contact => contact.Id == contactId)).ShouldBe(0);
    }
}
