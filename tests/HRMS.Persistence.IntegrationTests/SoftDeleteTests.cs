namespace HRMS.Persistence.IntegrationTests;

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

        (await assert.JobSeekers.CountAsync(seeker => seeker.Id == seekerId)).ShouldBe(0);

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

        await using (var act = fixture.CreateContext())
        {
            act.Remove(await act.JobAdvertisements.SingleAsync(ad => ad.Id == advertisementId));
            await act.SaveChangesAsync();
        }

        await using var assert = fixture.CreateContext();

        (await assert.JobAdvertisements.CountAsync(ad => ad.Id == advertisementId)).ShouldBe(0);

        (await assert.JobApplications.IgnoreQueryFilters()
            .CountAsync(application => application.JobAdvertisementId == advertisementId)).ShouldBe(1);
    }

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

        (await assert.Cvs.IgnoreQueryFilters().CountAsync(cv => cv.Id == cvId)).ShouldBe(1);
        (await assert.Educations.IgnoreQueryFilters().CountAsync(row => row.CvId == cvId)).ShouldBe(1);
    }

    // Sharp edge: the interceptor only rewrites the ISoftDeletable entry. Dependents EF already
    // marked Deleted by cascading through the tracker stay deleted, so an Include on a manager's
    // load path would silently turn a hide into a destroy.
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

        context.Remove(seeker);
        await context.SaveChangesAsync();

        await using var assert = fixture.CreateContext();

        assert.JobSeekers.IgnoreQueryFilters().Single(row => row.Id == seekerId).DeletedAt.ShouldNotBeNull();

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
