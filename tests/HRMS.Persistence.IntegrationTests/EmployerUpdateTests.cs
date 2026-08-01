using Application.Abstractions;
using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace HRMS.Persistence.IntegrationTests;

[Collection(PersistenceCollection.Name)]
public class EmployerUpdateTests(PostgresFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await fixture.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private async Task<Guid> GivenAnEmployerAsync(string email, params string[] departments)
    {
        await using var arrange = fixture.CreateContext();

        var employer = Given.Employer(email);

        foreach (var name in departments)
        {
            employer.Departments.Add(new Department { Name = name });
        }

        arrange.Add(employer);
        await arrange.SaveChangesAsync();

        return employer.Id;
    }

    [Fact]
    public async Task GetByIdAsync_Should_LoadDepartments()
    {
        var employerId = await GivenAnEmployerAsync("reads@test.local", "Platform", "Satış");

        var employer = await fixture.GetService<IEmployerRepository>().GetByIdAsync(employerId);

        employer.ShouldNotBeNull();
        employer.Departments.Count.ShouldBe(2);
        employer.Departments.Select(department => department.Name)
            .ShouldBe(["Platform", "Satış"], ignoreOrder: true);
    }

    [Fact]
    public async Task Update_Should_AddADepartment_ToAnEmployerThatHadNone()
    {
        var employerId = await GivenAnEmployerAsync("adds@test.local");

        await fixture.InScopeAsync(async services =>
        {
            var repository = services.GetRequiredService<IEmployerRepository>();
            var unitOfWork = services.GetRequiredService<IUnitOfWork>();

            var employer = await repository.GetByIdAsync(employerId);
            employer.ShouldNotBeNull();

            employer.Departments.Clear();
            employer.Departments.Add(new Department { Name = "Platform", NumberOfEmployees = 12 });

            await unitOfWork.SaveChangesAsync();

            return true;
        });

        await using var assert = fixture.CreateContext();
        var saved = await assert.Employers.Include(employer => employer.Departments)
            .SingleAsync(employer => employer.Id == employerId);

        var department = saved.Departments.ShouldHaveSingleItem();
        department.Name.ShouldBe("Platform");
        department.NumberOfEmployees.ShouldBe(12);
        department.EmployerId.ShouldBe(employerId);
    }

    [Fact]
    public async Task Update_Should_ReplaceTheDepartmentList()
    {
        var employerId = await GivenAnEmployerAsync("replaces@test.local", "Eski", "Daha eski");

        await fixture.InScopeAsync(async services =>
        {
            var repository = services.GetRequiredService<IEmployerRepository>();
            var unitOfWork = services.GetRequiredService<IUnitOfWork>();

            var employer = await repository.GetByIdAsync(employerId);

            employer!.Departments.Clear();
            employer.Departments.Add(new Department { Name = "Yeni" });

            await unitOfWork.SaveChangesAsync();

            return true;
        });

        await using var assert = fixture.CreateContext();
        var saved = await assert.Employers.Include(employer => employer.Departments)
            .SingleAsync(employer => employer.Id == employerId);

        saved.Departments.ShouldHaveSingleItem().Name.ShouldBe("Yeni");
    }
}
