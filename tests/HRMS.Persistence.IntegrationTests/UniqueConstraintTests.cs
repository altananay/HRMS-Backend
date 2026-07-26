using Domain.Entities;
using Npgsql;

namespace HRMS.Persistence.IntegrationTests;

[Collection(PersistenceCollection.Name)]
public class UniqueConstraintTests(PostgresFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await fixture.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static async Task ShouldViolateUniqueAsync(Func<Task> save)
    {
        var exception = await Should.ThrowAsync<DbUpdateException>(save);

        exception.InnerException.ShouldBeOfType<PostgresException>()
            .SqlState.ShouldBe(PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task Email_Should_BeUniqueAcrossEveryUserType()
    {
        await using (var arrange = fixture.CreateContext())
        {
            arrange.Add(Given.JobSeeker("duplicate@test.local"));
            await arrange.SaveChangesAsync();
        }

        await using var context = fixture.CreateContext();
        context.Add(Given.Employer("duplicate@test.local"));

        await ShouldViolateUniqueAsync(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Email_Should_BeUniqueRegardlessOfCase()
    {
        await using (var arrange = fixture.CreateContext())
        {
            arrange.Add(Given.JobSeeker("Mixed.Case@Test.Local"));
            await arrange.SaveChangesAsync();
        }

        await using var context = fixture.CreateContext();
        context.Add(Given.JobSeeker("mixed.case@test.local"));

        await ShouldViolateUniqueAsync(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Email_Should_BeReusableAfterTheAccountIsSoftDeleted()
    {
        await using (var arrange = fixture.CreateContext())
        {
            var seeker = Given.JobSeeker("recycled@test.local");
            arrange.Add(seeker);
            await arrange.SaveChangesAsync();

            arrange.Remove(seeker);
            await arrange.SaveChangesAsync();
        }

        await using var context = fixture.CreateContext();
        context.Add(Given.JobSeeker("recycled@test.local"));

        await Should.NotThrowAsync(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task JobSeeker_Should_HaveAtMostOneCv()
    {
        Guid seekerId;

        await using (var arrange = fixture.CreateContext())
        {
            var seeker = Given.JobSeeker("onecv@test.local");
            arrange.Add(seeker);
            arrange.Add(Given.Cv(seeker.Id));
            await arrange.SaveChangesAsync();
            seekerId = seeker.Id;
        }

        await using var context = fixture.CreateContext();
        context.Add(Given.Cv(seekerId));

        await ShouldViolateUniqueAsync(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task JobPositionName_Should_BeUnique()
    {
        await using (var arrange = fixture.CreateContext())
        {
            arrange.Add(Given.JobPosition("Backend Developer"));
            await arrange.SaveChangesAsync();
        }

        await using var context = fixture.CreateContext();
        context.Add(Given.JobPosition("Backend Developer"));

        await ShouldViolateUniqueAsync(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Seeker_Should_ApplyToAnAdvertisementOnlyOnce()
    {
        Guid advertisementId, seekerId;

        await using (var arrange = fixture.CreateContext())
        {
            var employer = Given.Employer("dupe-employer@test.local");
            var seeker = Given.JobSeeker("dupe-seeker@test.local");
            var position = Given.JobPosition("Duplicate Tester");
            arrange.AddRange(employer, seeker, position);

            var advertisement = Given.JobAdvertisement(employer.Id, position.Id);
            arrange.Add(advertisement);
            arrange.Add(Given.JobApplication(advertisement.Id, seeker.Id));
            await arrange.SaveChangesAsync();

            advertisementId = advertisement.Id;
            seekerId = seeker.Id;
        }

        await using var context = fixture.CreateContext();
        context.Add(Given.JobApplication(advertisementId, seekerId));

        await ShouldViolateUniqueAsync(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Seeker_Should_ApplyToSeveralDifferentAdvertisements()
    {
        await using var context = fixture.CreateContext();

        var employer = Given.Employer("multi-employer@test.local");
        var seeker = Given.JobSeeker("multi-seeker@test.local");
        var position = Given.JobPosition("Multi Tester");
        context.AddRange(employer, seeker, position);

        var first = Given.JobAdvertisement(employer.Id, position.Id, "İlan 1");
        var second = Given.JobAdvertisement(employer.Id, position.Id, "İlan 2");
        context.AddRange(first, second);
        context.Add(Given.JobApplication(first.Id, seeker.Id));
        context.Add(Given.JobApplication(second.Id, seeker.Id));

        await Should.NotThrowAsync(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task NationalId_Should_BeUniqueWhenSupplied()
    {
        await using (var arrange = fixture.CreateContext())
        {
            arrange.Add(Given.JobSeeker("tckn-first@test.local", nationalId: "12345678901"));
            await arrange.SaveChangesAsync();
        }

        await using var context = fixture.CreateContext();
        context.Add(Given.JobSeeker("tckn-second@test.local", nationalId: "12345678901"));

        await ShouldViolateUniqueAsync(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task NationalId_Should_BeOmittableByManySeekers()
    {
        await using var context = fixture.CreateContext();

        context.Add(Given.JobSeeker("no-tckn-1@test.local"));
        context.Add(Given.JobSeeker("no-tckn-2@test.local"));
        context.Add(Given.JobSeeker("no-tckn-3@test.local"));

        await Should.NotThrowAsync(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task RefreshTokenHash_Should_BeUnique()
    {
        Guid userId;

        await using (var arrange = fixture.CreateContext())
        {
            var seeker = Given.JobSeeker("token@test.local");
            arrange.Add(seeker);
            arrange.Add(new RefreshToken
            {
                UserId = seeker.Id,
                TokenHash = "identical-hash",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
            await arrange.SaveChangesAsync();
            userId = seeker.Id;
        }

        await using var context = fixture.CreateContext();
        context.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = "identical-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await ShouldViolateUniqueAsync(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task RoleName_Should_BeUnique()
    {
        await using (var arrange = fixture.CreateContext())
        {
            arrange.Add(new Role { Name = "duplicate-role" });
            await arrange.SaveChangesAsync();
        }

        await using var context = fixture.CreateContext();
        context.Add(new Role { Name = "duplicate-role" });

        await ShouldViolateUniqueAsync(() => context.SaveChangesAsync());
    }
}
