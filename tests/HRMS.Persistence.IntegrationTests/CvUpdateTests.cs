using Application.Abstractions;
using Application.Abstractions.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace HRMS.Persistence.IntegrationTests;

/// <summary>
/// Saving a CV that already exists and gaining a child row in the same change set.
/// </summary>
/// <remarks>
/// This is the exact shape <see cref="Application.Services.CvManager.UpdateAsync"/> produces, and it
/// used to fail: every attempt threw <see cref="DbUpdateConcurrencyException"/> ("expected to affect
/// 1 row(s), but actually affected 0"), so no job seeker could add an education, a job, a language or
/// a project to a saved CV. Nothing covered it — the existing suite exercises create and delete, and
/// an update with no children happens to work.
/// </remarks>
[Collection(PersistenceCollection.Name)]
public class CvUpdateTests(PostgresFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await fixture.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private async Task<Guid> GivenACvAsync(string email)
    {
        await using var arrange = fixture.CreateContext();

        var seeker = Given.JobSeeker(email);
        arrange.Add(seeker);
        arrange.Add(Given.Cv(seeker.Id));
        await arrange.SaveChangesAsync();

        return seeker.Id;
    }

    [Fact]
    public async Task Update_Should_AddAChildRow_ToAnExistingCv()
    {
        var seekerId = await GivenACvAsync("adds-education@test.local");

        await fixture.InScopeAsync(async services =>
        {
            var repository = services.GetRequiredService<ICvRepository>();
            var unitOfWork = services.GetRequiredService<IUnitOfWork>();

            var cv = await repository.GetByJobSeekerIdAsync(seekerId);
            cv.ShouldNotBeNull();

            cv.Information = "Updated.";
            cv.Educations.Add(new Education
            {
                School = "ODTÜ",
                Major = "Bilgisayar Mühendisliği",
                IsGraduated = true
            });

            await unitOfWork.SaveChangesAsync();

            return true;
        });

        await using var assert = fixture.CreateContext();
        var saved = await assert.Cvs.Include(cv => cv.Educations)
            .SingleAsync(cv => cv.JobSeekerId == seekerId);

        saved.Information.ShouldBe("Updated.");

        // `Given.Cv` already carries an education, so the new one lands *beside* it — which is the
        // case that matters: adding to a collection that is already populated is exactly what failed.
        saved.Educations.Count.ShouldBe(2);
        saved.Educations.Select(education => education.School).ShouldContain("ODTÜ");
        saved.Educations.ShouldAllBe(education => education.Id != Guid.Empty);
    }

    [Fact]
    public async Task Update_Should_ReplaceEveryCollection_InOneSaveChanges()
    {
        // The full shape of `ApplyTo`: clear each collection, re-add, and mutate the principal — all
        // against a CV that already has rows. Four child tables and the parent in a single batch is
        // what made the ordering matter.
        var seekerId = await GivenACvAsync("replaces-all@test.local");

        await fixture.InScopeAsync(async services =>
        {
            var repository = services.GetRequiredService<ICvRepository>();
            var unitOfWork = services.GetRequiredService<IUnitOfWork>();

            var cv = await repository.GetByJobSeekerIdAsync(seekerId);
            cv!.Educations.Add(new Education { School = "First", Major = "M", IsGraduated = false });
            cv.JobExperiences.Add(new JobExperience { CompanyName = "First", Position = "P" });
            await unitOfWork.SaveChangesAsync();

            return true;
        });

        await fixture.InScopeAsync(async services =>
        {
            var repository = services.GetRequiredService<ICvRepository>();
            var unitOfWork = services.GetRequiredService<IUnitOfWork>();

            var cv = await repository.GetByJobSeekerIdAsync(seekerId);
            cv.ShouldNotBeNull();

            cv.Information = "Second pass.";
            cv.Skills = ["C#", "PostgreSQL"];

            cv.Educations.Clear();
            cv.Educations.Add(new Education { School = "Second", Major = "M2", IsGraduated = true });

            cv.JobExperiences.Clear();
            cv.JobExperiences.Add(new JobExperience { CompanyName = "Second", Position = "P2" });

            cv.Languages.Clear();
            cv.Languages.Add(new CvLanguage { Name = "İngilizce", Level = LanguageLevel.Advanced });

            cv.Projects.Clear();
            cv.Projects.Add(new CvProject { Name = "hrms" });

            await unitOfWork.SaveChangesAsync();

            return true;
        });

        await using var assert = fixture.CreateContext();
        var saved = await assert.Cvs
            .Include(cv => cv.Educations)
            .Include(cv => cv.JobExperiences)
            .Include(cv => cv.Languages)
            .Include(cv => cv.Projects)
            .SingleAsync(cv => cv.JobSeekerId == seekerId);

        saved.Information.ShouldBe("Second pass.");
        saved.Skills.ShouldBe(["C#", "PostgreSQL"]);
        saved.Educations.ShouldHaveSingleItem().School.ShouldBe("Second");
        saved.JobExperiences.ShouldHaveSingleItem().CompanyName.ShouldBe("Second");
        saved.Languages.ShouldHaveSingleItem().Name.ShouldBe("İngilizce");
        saved.Projects.ShouldHaveSingleItem().Name.ShouldBe("hrms");
    }
}
