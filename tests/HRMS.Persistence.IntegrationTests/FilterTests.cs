using Application.Abstractions.Repositories;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;

namespace HRMS.Persistence.IntegrationTests;

[Collection(PersistenceCollection.Name)]
public class FilterTests(PostgresFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await fixture.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private async Task<(Guid EmployerId, Guid RivalId)> GivenABoardAsync()
    {
        await using var context = fixture.CreateContext();

        var employer = Given.Employer("board@test.local", "Acme");
        var rival = Given.Employer("rival@test.local", "Rival Co");
        var position = Given.JobPosition("Backend Developer");
        context.AddRange(employer, rival, position);

        var react = Given.JobAdvertisement(employer.Id, position.Id, "React Developer");
        react.Skills = ["React", "TypeScript"];
        react.City = "İstanbul";
        react.Description = "Modern bir arayüz ekibi arıyoruz.";

        var dotnet = Given.JobAdvertisement(employer.Id, position.Id, "Backend Developer");
        dotnet.Skills = ["csharp", "PostgreSQL"];
        dotnet.City = "Ankara";
        dotnet.Description = "Dağıtık sistemlerde deneyimli backend geliştirici.";

        var passive = Given.JobAdvertisement(rival.Id, position.Id, "React Native Developer");
        passive.Skills = ["React"];
        passive.City = "izmir";
        passive.IsActive = false;
        passive.Description = "Mobil tarafta React deneyimi beklenmektedir.";

        context.AddRange(react, dotnet, passive);
        await context.SaveChangesAsync();

        return (employer.Id, rival.Id);
    }

    private Task<PagedResult<JobAdvertisement>> QueryAsync(JobAdvertisementFilter filter)
        => fixture.GetService<IJobAdvertisementRepository>().GetPagedAsync(new PageRequest(), filter);

    // ---------------------------------------------------------------------------------------------
    // Job advertisements
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public async Task Skill_Should_MatchAnElementOfTheArray()
    {
        await GivenABoardAsync();

        var page = await QueryAsync(new JobAdvertisementFilter { Skill = "React" });

        page.TotalCount.ShouldBe(2);
        page.Items.ShouldAllBe(advertisement => advertisement.Skills.Contains("React"));
    }

    /// <summary>
    /// Array containment is exact, not a substring: "React" must not drag in "React Native" as a
    /// skill, and a partial word must match nothing.
    /// </summary>
    [Fact]
    public async Task Skill_Should_NotMatchPartially()
    {
        await GivenABoardAsync();

        (await QueryAsync(new JobAdvertisementFilter { Skill = "Rea" })).TotalCount.ShouldBe(0);
        (await QueryAsync(new JobAdvertisementFilter { Skill = "Script" })).TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task City_Should_MatchCaseInsensitively()
    {
        await GivenABoardAsync();

        (await QueryAsync(new JobAdvertisementFilter { City = "istanbul" })).TotalCount.ShouldBe(1);
        (await QueryAsync(new JobAdvertisementFilter { City = "İSTANBUL" })).TotalCount.ShouldBe(1);
        (await QueryAsync(new JobAdvertisementFilter { City = "İzmir" })).TotalCount.ShouldBe(1);
    }

    /// <summary>
    /// Each term below appears in exactly one place, so the counts prove the OR really spans both
    /// columns rather than one of them happening to carry every match.
    /// </summary>
    [Fact]
    public async Task Search_Should_CoverBothTitleAndDescription()
    {
        await GivenABoardAsync();

        // Title only.
        (await QueryAsync(new JobAdvertisementFilter { Search = "Native" })).TotalCount.ShouldBe(1);

        // Description only.
        (await QueryAsync(new JobAdvertisementFilter { Search = "dağıtık" })).TotalCount.ShouldBe(1);

        // Every title, no description.
        (await QueryAsync(new JobAdvertisementFilter { Search = "developer" })).TotalCount.ShouldBe(3);

        (await QueryAsync(new JobAdvertisementFilter { Search = "bulunmayan" })).TotalCount.ShouldBe(0);
    }

    /// <summary>
    /// LIKE wildcards in user input must be escaped. Unescaped, a search for "%" matches every row —
    /// a search box that silently returns the whole table.
    /// </summary>
    [Fact]
    public async Task Search_Should_TreatWildcardsAsLiteralText()
    {
        await GivenABoardAsync();

        (await QueryAsync(new JobAdvertisementFilter { Search = "%" })).TotalCount.ShouldBe(0);
        (await QueryAsync(new JobAdvertisementFilter { Search = "_" })).TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Filters_Should_Combine()
    {
        var (employerId, _) = await GivenABoardAsync();

        var page = await QueryAsync(new JobAdvertisementFilter
        {
            EmployerId = employerId,
            IsActive = true,
            Skill = "React"
        });

        page.TotalCount.ShouldBe(1);
        page.Items[0].Title.ShouldBe("React Developer");
    }

    [Fact]
    public async Task NoFilter_Should_ReturnEverything()
    {
        await GivenABoardAsync();

        (await QueryAsync(JobAdvertisementFilter.None)).TotalCount.ShouldBe(3);
    }

    // ---------------------------------------------------------------------------------------------
    // Job applications
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// Backs the employer's per-listing view. Without it that screen has to pull the employer's whole
    /// application page and filter client-side, which is wrong past the first hundred rows.
    /// </summary>
    [Fact]
    public async Task JobAdvertisementId_Should_NarrowToOneListing()
    {
        Guid firstAdvertisementId;

        await using (var arrange = fixture.CreateContext())
        {
            var employer = Given.Employer("apps@test.local");
            var first = Given.JobSeeker("first@test.local");
            var second = Given.JobSeeker("second@test.local");
            var position = Given.JobPosition("Applications Position");
            arrange.AddRange(employer, first, second, position);

            var advertisementA = Given.JobAdvertisement(employer.Id, position.Id, "İlan A");
            var advertisementB = Given.JobAdvertisement(employer.Id, position.Id, "İlan B");
            arrange.AddRange(advertisementA, advertisementB);

            arrange.Add(Given.JobApplication(advertisementA.Id, first.Id));
            arrange.Add(Given.JobApplication(advertisementA.Id, second.Id));
            arrange.Add(Given.JobApplication(advertisementB.Id, first.Id));
            await arrange.SaveChangesAsync();

            firstAdvertisementId = advertisementA.Id;
        }

        var repository = fixture.GetService<IJobApplicationRepository>();

        var scoped = await repository.GetPagedAsync(
            new PageRequest(), new JobApplicationFilter { JobAdvertisementId = firstAdvertisementId });

        scoped.TotalCount.ShouldBe(2);
        scoped.Items.ShouldAllBe(application => application.JobAdvertisementId == firstAdvertisementId);

        (await repository.GetPagedAsync(new PageRequest(), JobApplicationFilter.None))
            .TotalCount.ShouldBe(3);
    }

    [Fact]
    public async Task JobAdvertisementId_Should_CombineWithStatus()
    {
        Guid advertisementId;

        await using (var arrange = fixture.CreateContext())
        {
            var employer = Given.Employer("status@test.local");
            var first = Given.JobSeeker("s1@test.local");
            var second = Given.JobSeeker("s2@test.local");
            var position = Given.JobPosition("Status Position");
            arrange.AddRange(employer, first, second, position);

            var advertisement = Given.JobAdvertisement(employer.Id, position.Id);
            arrange.Add(advertisement);

            var accepted = Given.JobApplication(advertisement.Id, first.Id);
            accepted.Status = JobApplicationStatus.Accepted;
            arrange.Add(accepted);
            arrange.Add(Given.JobApplication(advertisement.Id, second.Id));
            await arrange.SaveChangesAsync();

            advertisementId = advertisement.Id;
        }

        var page = await fixture.GetService<IJobApplicationRepository>().GetPagedAsync(
            new PageRequest(),
            new JobApplicationFilter
            {
                JobAdvertisementId = advertisementId,
                Status = JobApplicationStatus.Accepted
            });

        page.TotalCount.ShouldBe(1);
        page.Items[0].Status.ShouldBe(JobApplicationStatus.Accepted);
    }
}
