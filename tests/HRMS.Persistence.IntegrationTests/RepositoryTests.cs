using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.Persistence.IntegrationTests;

[Collection(PersistenceCollection.Name)]
public class RepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await fixture.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task GetByJobSeekerIdAsync_Should_IncludeTheOwner()
    {
        Guid seekerId;

        await using (var arrange = fixture.CreateContext())
        {
            var seeker = Given.JobSeeker("owner@test.local");
            arrange.Add(seeker);
            arrange.Add(Given.Cv(seeker.Id));
            await arrange.SaveChangesAsync();
            seekerId = seeker.Id;
        }

        var cv = await fixture.GetService<ICvRepository>().GetByJobSeekerIdAsync(seekerId);

        cv.ShouldNotBeNull();
        cv.JobSeeker.ShouldNotBeNull("the Response projection dereferences this");
        cv.JobSeeker.Email.ShouldBe("owner@test.local");
    }

    [Fact]
    public async Task GetByJobSeekerIdAsync_Should_IncludeEveryChildCollection()
    {
        Guid seekerId;

        await using (var arrange = fixture.CreateContext())
        {
            var seeker = Given.JobSeeker("children-loaded@test.local");
            arrange.Add(seeker);
            arrange.Add(Given.Cv(seeker.Id));
            await arrange.SaveChangesAsync();
            seekerId = seeker.Id;
        }

        var cv = await fixture.GetService<ICvRepository>().GetByJobSeekerIdAsync(seekerId);

        cv.ShouldNotBeNull();
        cv.Educations.Count.ShouldBe(1);
        cv.JobExperiences.Count.ShouldBe(1);
        cv.Languages.Count.ShouldBe(1);
        cv.Projects.Count.ShouldBe(1);
        cv.Files.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetPagedAsync_Should_IncludeTheOwnerOnEveryRow()
    {
        await using (var arrange = fixture.CreateContext())
        {
            var first = Given.JobSeeker("paged-1@test.local");
            var second = Given.JobSeeker("paged-2@test.local");
            arrange.AddRange(first, second);
            arrange.Add(Given.Cv(first.Id));
            arrange.Add(Given.Cv(second.Id));
            await arrange.SaveChangesAsync();
        }

        var page = await fixture.GetService<ICvRepository>().GetPagedAsync(new PageRequest());

        page.Items.Count.ShouldBe(2);
        page.Items.ShouldAllBe(cv => cv.JobSeeker != null);
    }

    [Fact]
    public async Task ExistsForJobSeekerAsync_Should_ReflectWhetherACvWasCreated()
    {
        var repository = fixture.GetService<ICvRepository>();
        Guid withCv, withoutCv;

        await using (var arrange = fixture.CreateContext())
        {
            var first = Given.JobSeeker("has-cv@test.local");
            var second = Given.JobSeeker("no-cv@test.local");
            arrange.AddRange(first, second);
            arrange.Add(Given.Cv(first.Id));
            await arrange.SaveChangesAsync();

            withCv = first.Id;
            withoutCv = second.Id;
        }

        (await repository.ExistsForJobSeekerAsync(withCv)).ShouldBeTrue();
        (await repository.ExistsForJobSeekerAsync(withoutCv)).ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsForEmployerAndSeekerAsync_Should_FollowTheAdvertisementToItsOwner()
    {
        var repository = fixture.GetService<IJobApplicationRepository>();
        Guid employerId, rivalId, applicantId, strangerId;

        await using (var arrange = fixture.CreateContext())
        {
            var employer = Given.Employer("receiving@test.local");
            var rival = Given.Employer("rival@test.local", "Rival Co");
            var applicant = Given.JobSeeker("applicant@test.local");
            var stranger = Given.JobSeeker("stranger@test.local");
            var position = Given.JobPosition("Predicate Position");
            arrange.AddRange(employer, rival, applicant, stranger, position);

            var advertisement = Given.JobAdvertisement(employer.Id, position.Id);
            arrange.Add(advertisement);
            arrange.Add(Given.JobApplication(advertisement.Id, applicant.Id));
            await arrange.SaveChangesAsync();

            employerId = employer.Id;
            rivalId = rival.Id;
            applicantId = applicant.Id;
            strangerId = stranger.Id;
        }

        (await repository.ExistsForEmployerAndSeekerAsync(employerId, applicantId)).ShouldBeTrue();

        (await repository.ExistsForEmployerAndSeekerAsync(rivalId, applicantId)).ShouldBeFalse();
        (await repository.ExistsForEmployerAndSeekerAsync(employerId, strangerId)).ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsForSeekerAndAdvertisementAsync_Should_BackTheDuplicateApplicationRule()
    {
        var repository = fixture.GetService<IJobApplicationRepository>();
        Guid advertisementId, applicantId, otherId;

        await using (var arrange = fixture.CreateContext())
        {
            var employer = Given.Employer("dup-check@test.local");
            var applicant = Given.JobSeeker("dup-applicant@test.local");
            var other = Given.JobSeeker("dup-other@test.local");
            var position = Given.JobPosition("Duplicate Check Position");
            arrange.AddRange(employer, applicant, other, position);

            var advertisement = Given.JobAdvertisement(employer.Id, position.Id);
            arrange.Add(advertisement);
            arrange.Add(Given.JobApplication(advertisement.Id, applicant.Id));
            await arrange.SaveChangesAsync();

            advertisementId = advertisement.Id;
            applicantId = applicant.Id;
            otherId = other.Id;
        }

        (await repository.ExistsForSeekerAndAdvertisementAsync(applicantId, advertisementId)).ShouldBeTrue();
        (await repository.ExistsForSeekerAndAdvertisementAsync(otherId, advertisementId)).ShouldBeFalse();
    }

    [Fact]
    public async Task GetForAuthenticationAsync_Should_ReturnTheUserWithTheirRoles()
    {
        await using (var arrange = fixture.CreateContext())
        {
            var role = new Role { Name = "jobseeker" };
            var seeker = Given.JobSeeker("auth@test.local");
            arrange.AddRange(role, seeker);
            arrange.Add(new UserRole { UserId = seeker.Id, RoleId = role.Id });
            await arrange.SaveChangesAsync();
        }

        var found = await fixture.GetService<IUserRepository>().GetForAuthenticationAsync("auth@test.local");

        found.ShouldNotBeNull();
        found.Value.User.Email.ShouldBe("auth@test.local");
        found.Value.Roles.ShouldContain("jobseeker");
    }

    [Fact]
    public async Task GetForAuthenticationAsync_Should_MatchRegardlessOfCase()
    {
        await using (var arrange = fixture.CreateContext())
        {
            arrange.Add(Given.JobSeeker("CaseTest@Test.Local"));
            await arrange.SaveChangesAsync();
        }

        var found = await fixture.GetService<IUserRepository>().GetForAuthenticationAsync("casetest@test.local");

        found.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetForAuthenticationAsync_Should_NotFindASoftDeletedAccount()
    {
        await using (var arrange = fixture.CreateContext())
        {
            var seeker = Given.JobSeeker("closed@test.local");
            arrange.Add(seeker);
            await arrange.SaveChangesAsync();

            arrange.Remove(seeker);
            await arrange.SaveChangesAsync();
        }

        (await fixture.GetService<IUserRepository>().GetForAuthenticationAsync("closed@test.local"))
            .ShouldBeNull();
    }

    [Fact]
    public async Task GetSecurityStateAsync_Should_ReturnTheStampAndActiveFlag()
    {
        Guid seekerId;
        Guid stamp;

        await using (var arrange = fixture.CreateContext())
        {
            var seeker = Given.JobSeeker("stamp@test.local");
            seeker.IsActive = false;
            arrange.Add(seeker);
            await arrange.SaveChangesAsync();

            seekerId = seeker.Id;
            stamp = seeker.SecurityStamp;
        }

        var state = await fixture.GetService<IUserRepository>().GetSecurityStateAsync(seekerId);

        state.ShouldNotBeNull();
        state.SecurityStamp.ShouldBe(stamp);
        state.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task ResolveOrCreateAsync_Should_ReuseAnExistingPositionRatherThanInsertAgain()
    {
        var first = await fixture.InScopeAsync(async services =>
        {
            var created = await services.GetRequiredService<IJobPositionRepository>()
                .ResolveOrCreateAsync("Backend Developer");

            await services.GetRequiredService<IUnitOfWork>().SaveChangesAsync();

            return created.Id;
        });

        var second = await fixture.InScopeAsync(async services =>
        {
            var resolved = await services.GetRequiredService<IJobPositionRepository>()
                .ResolveOrCreateAsync("Backend Developer");

            await services.GetRequiredService<IUnitOfWork>().SaveChangesAsync();

            return resolved.Id;
        });

        second.ShouldBe(first);

        await using var assert = fixture.CreateContext();
        (await assert.JobPositions.CountAsync(position => position.Name == "Backend Developer")).ShouldBe(1);
    }

    /// <summary>
    /// The race the check-then-insert version lost: two requests naming the same brand-new position at
    /// the same moment. Both read "no such row"; without the upsert the second insert violates
    /// <c>ix_job_positions_name</c> and the publish answers 500.
    /// </summary>
    [Fact]
    public async Task ResolveOrCreateAsync_Should_SurviveTwoRequestsCreatingTheSameNameAtOnce()
    {
        const int callers = 4;

        // The barrier is what makes this deterministic rather than a coin toss. Every caller must have
        // resolved — and, in the old check-then-insert version, decided to insert — before any of them
        // saves. Firing four tasks and hoping they overlap passes even against the broken code, which
        // is exactly how this defect survived being "tested".
        using var everyoneHasResolved = new Barrier(callers);

        // Separate scopes, so separate DbContexts and separate connections: the shape of concurrent
        // requests. One context would serialize them and prove nothing.
        var resolve = () => Task.Run(() => fixture.InScopeAsync(async services =>
        {
            var position = await services.GetRequiredService<IJobPositionRepository>()
                .ResolveOrCreateAsync("Site Reliability Engineer");

            everyoneHasResolved.SignalAndWait();

            await services.GetRequiredService<IUnitOfWork>().SaveChangesAsync();

            return position.Id;
        }));

        var ids = await Task.WhenAll(Enumerable.Range(0, callers).Select(_ => resolve()));

        // Every caller must come back with the row that actually won, not with an id it invented.
        ids.Distinct().Count().ShouldBe(1);

        await using var assert = fixture.CreateContext();
        (await assert.JobPositions.CountAsync(position => position.Name == "Site Reliability Engineer"))
            .ShouldBe(1);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_Should_MatchAnExistingNameRegardlessOfCase()
    {
        // `name` is citext and the upsert's conflict target is that column, so "backend developer"
        // must find "Backend Developer" rather than trying to insert a second row.
        var first = await fixture.InScopeAsync(async services =>
        {
            var created = await services.GetRequiredService<IJobPositionRepository>()
                .ResolveOrCreateAsync("Backend Developer");

            await services.GetRequiredService<IUnitOfWork>().SaveChangesAsync();

            return created.Id;
        });

        var second = await fixture.InScopeAsync(async services =>
            (await services.GetRequiredService<IJobPositionRepository>()
                .ResolveOrCreateAsync("  backend DEVELOPER  ")).Id);

        second.ShouldBe(first);

        await using var assert = fixture.CreateContext();
        (await assert.JobPositions.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_Should_CreateASeparatePositionForADifferentName()
    {
        foreach (var name in new[] { "Backend Developer", "Frontend Developer" })
        {
            await fixture.InScopeAsync(async services =>
            {
                await services.GetRequiredService<IJobPositionRepository>().ResolveOrCreateAsync(name);
                return await services.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
            });
        }

        await using var assert = fixture.CreateContext();
        (await assert.JobPositions.CountAsync()).ShouldBe(2);
    }
}
