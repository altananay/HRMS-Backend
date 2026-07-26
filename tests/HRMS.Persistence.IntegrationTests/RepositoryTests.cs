using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.Persistence.IntegrationTests;

/// <summary>
/// The repository methods, against a real database.
/// </summary>
/// <remarks>
/// The unit tests substitute these interfaces, which is exactly why they need covering somewhere:
/// a substituted repository returns whatever the test says it returns, so a query that forgets an
/// Include or gets a join backwards passes every manager test and fails in production.
///
/// That is not hypothetical here — see <see cref="GetByJobSeekerIdAsync_Should_IncludeTheOwner"/>.
/// </remarks>
[Collection(PersistenceCollection.Name)]
public class RepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await fixture.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---------------------------------------------------------------------------------------------
    // CVs
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// Regression guard for a defect that reached the running application: the query omitted the
    /// JobSeeker navigation while the DTO projection read the owner's name, email and date of birth
    /// off it, so every CV read — the owner's own included — answered 500 with a
    /// NullReferenceException. No test touched a CV read, at any level, so nothing caught it.
    /// </summary>
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
        cv.JobSeeker.ShouldNotBeNull("the DTO projection dereferences this");
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

    /// <summary>The admin list projects the same DTO, so it needs the same navigations loaded.</summary>
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

    // ---------------------------------------------------------------------------------------------
    // Applications
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// The authorization predicate behind CV access: an employer earns the right to read a candidate
    /// by having received an application from them. It walks applications → advertisement → employer,
    /// so a join in the wrong direction would grant or deny access to the wrong people entirely.
    /// </summary>
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

        // A different employer, a different seeker: neither pairing exists.
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

    // ---------------------------------------------------------------------------------------------
    // Users
    // ---------------------------------------------------------------------------------------------

    /// <summary>Sign-in needs the role names in the same round trip that finds the user.</summary>
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

    /// <summary>citext again — sign-in must not depend on how the address was typed.</summary>
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

    /// <summary>Read on every authenticated request, so it stays a two-column projection.</summary>
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

    // ---------------------------------------------------------------------------------------------
    // Job positions
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// Resolve-or-create is what keeps positions a shared lookup. The old Add created one per
    /// advertisement and deleted it along with the advertisement.
    /// </summary>
    [Fact]
    public async Task ResolveOrCreateAsync_Should_ReuseAnExistingPositionRatherThanInsertAgain()
    {
        // One scope per call, and the save goes through the same scope's unit of work — resolving the
        // repository and the context separately would give each its own DbContext, and the insert
        // would never be committed.
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

    /// <summary>A different name is a different position, not a reuse.</summary>
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
