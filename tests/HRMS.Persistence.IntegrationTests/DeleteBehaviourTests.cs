using Domain.Entities;
using Npgsql;

namespace HRMS.Persistence.IntegrationTests;

[Collection(PersistenceCollection.Name)]
public class DeleteBehaviourTests(PostgresFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await fixture.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task DeletingACv_Should_TakeItsChildRowsWithIt()
    {
        Guid cvId;

        await using (var arrange = fixture.CreateContext())
        {
            var seeker = Given.JobSeeker("cascade@test.local");
            arrange.Add(seeker);

            var cv = Given.Cv(seeker.Id);
            arrange.Add(cv);
            await arrange.SaveChangesAsync();
            cvId = cv.Id;
        }

        await using (var act = fixture.CreateContext())
        {
            act.Remove(await act.Cvs.SingleAsync(cv => cv.Id == cvId));
            await act.SaveChangesAsync();
        }

        await using var assert = fixture.CreateContext();

        (await assert.Cvs.CountAsync(cv => cv.Id == cvId)).ShouldBe(0);
        (await assert.Educations.CountAsync(row => row.CvId == cvId)).ShouldBe(0);
        (await assert.JobExperiences.CountAsync(row => row.CvId == cvId)).ShouldBe(0);
        (await assert.CvLanguages.CountAsync(row => row.CvId == cvId)).ShouldBe(0);
        (await assert.CvProjects.CountAsync(row => row.CvId == cvId)).ShouldBe(0);
        (await assert.CvFiles.CountAsync(row => row.CvId == cvId)).ShouldBe(0);
    }

    [Fact]
    public async Task DeletingAJobPositionInUse_Should_BeRefusedByTheDatabase()
    {
        Guid positionId;

        await using (var arrange = fixture.CreateContext())
        {
            var employer = Given.Employer("restrict@test.local");
            var position = Given.JobPosition("Restricted Position");
            arrange.AddRange(employer, position);
            arrange.Add(Given.JobAdvertisement(employer.Id, position.Id));
            await arrange.SaveChangesAsync();
            positionId = position.Id;
        }

        await using var context = fixture.CreateContext();
        context.Remove(await context.JobPositions.SingleAsync(position => position.Id == positionId));

        var exception = await Should.ThrowAsync<DbUpdateException>(() => context.SaveChangesAsync());

        exception.InnerException.ShouldBeOfType<PostgresException>()
            .SqlState.ShouldBe(PostgresErrorCodes.ForeignKeyViolation);
    }

    [Fact]
    public async Task DeletingAnUnusedJobPosition_Should_Succeed()
    {
        Guid positionId;

        await using (var arrange = fixture.CreateContext())
        {
            var position = Given.JobPosition("Unused Position");
            arrange.Add(position);
            await arrange.SaveChangesAsync();
            positionId = position.Id;
        }

        await using var context = fixture.CreateContext();
        context.Remove(await context.JobPositions.SingleAsync(position => position.Id == positionId));

        await Should.NotThrowAsync(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task HardDeletingAUser_Should_TakeTheirTokensAndRoleAssignments()
    {
        Guid userId;

        await using (var arrange = fixture.CreateContext())
        {
            var role = new Role { Name = "cascade-role" };
            var seeker = Given.JobSeeker("cascade-user@test.local");
            arrange.AddRange(role, seeker);
            arrange.Add(new UserRole { UserId = seeker.Id, RoleId = role.Id });
            arrange.Add(new RefreshToken
            {
                UserId = seeker.Id,
                TokenHash = "cascade-token",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
            await arrange.SaveChangesAsync();
            userId = seeker.Id;
        }

        await using (var act = fixture.CreateContext())
        {
            await act.Database.ExecuteSqlRawAsync("delete from users where id = {0}", userId);
        }

        await using var assert = fixture.CreateContext();

        (await assert.RefreshTokens.IgnoreQueryFilters().CountAsync(token => token.UserId == userId)).ShouldBe(0);
        (await assert.UserRoles.IgnoreQueryFilters().CountAsync(link => link.UserId == userId)).ShouldBe(0);
        (await assert.JobSeekers.IgnoreQueryFilters().CountAsync(seeker => seeker.Id == userId)).ShouldBe(0);
    }

    [Fact]
    public async Task JobSeekerRow_Should_ShareTheIdentityOfItsUserRow()
    {
        await using var context = fixture.CreateContext();

        var seeker = Given.JobSeeker("tpt@test.local");
        context.Add(seeker);
        await context.SaveChangesAsync();

        var userRows = await context.Database
            .SqlQuery<int>($"select count(*)::int as \"Value\" from users where id = {seeker.Id}")
            .SingleAsync();

        var seekerRows = await context.Database
            .SqlQuery<int>($"select count(*)::int as \"Value\" from job_seekers where id = {seeker.Id}")
            .SingleAsync();

        userRows.ShouldBe(1);
        seekerRows.ShouldBe(1);
    }
}
