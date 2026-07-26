using Domain.Entities;
using Npgsql;

namespace HRMS.Persistence.IntegrationTests;

/// <summary>
/// Rules the database enforces, rather than rules the application remembers to check.
/// </summary>
/// <remarks>
/// MongoDB enforced none of these. Duplicate applications, two CVs for one seeker and a job position
/// created afresh for every advertisement were all reachable, because the only guard was a
/// hand-written existence check that a concurrent request could slip past between the read and the
/// write. A unique index cannot be raced.
/// </remarks>
[Collection(PersistenceCollection.Name)]
public class UniqueConstraintTests(PostgresFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await fixture.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>PostgreSQL's unique_violation. Anything else is a different failure.</summary>
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

        // A different subtype in the TPT hierarchy still shares the users table.
        await using var context = fixture.CreateContext();
        context.Add(Given.Employer("duplicate@test.local"));

        await ShouldViolateUniqueAsync(() => context.SaveChangesAsync());
    }

    /// <summary>citext, so the uniqueness is on the address rather than on its casing.</summary>
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

    /// <summary>
    /// The point of the partial index: the unique constraint is filtered on <c>deleted_at IS NULL</c>,
    /// so closing an account releases its address instead of burning it forever.
    /// </summary>
    [Fact]
    public async Task Email_Should_BeReusableAfterTheAccountIsSoftDeleted()
    {
        await using (var arrange = fixture.CreateContext())
        {
            var seeker = Given.JobSeeker("recycled@test.local");
            arrange.Add(seeker);
            await arrange.SaveChangesAsync();

            arrange.Remove(seeker);           // interceptor rewrites this as a soft delete
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

    /// <summary>
    /// What turns JobPosition into a shared lookup. Add used to create a new position per
    /// advertisement and delete it again with the advertisement.
    /// </summary>
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

    /// <summary>Applying to a second advertisement is the same seeker, a different pair.</summary>
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

    /// <summary>
    /// Filtered on <c>national_id IS NOT NULL</c>, so any number of seekers may leave it unset while
    /// a supplied one still cannot be claimed twice.
    /// </summary>
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

    /// <summary>
    /// Refresh tokens are stored hashed and looked up by that hash, so a collision would let one
    /// token resolve to another user's session.
    /// </summary>
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
