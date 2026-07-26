namespace HRMS.Persistence.IntegrationTests;

/// <summary>
/// Timestamps, stamped centrally.
/// </summary>
/// <remarks>
/// These were hand-written in every Add and Update method before — and several used
/// <c>DateTime.Now</c> rather than UtcNow, so on a UTC+3 server the values were three hours ahead of
/// everything else in the database. Doing it in one interceptor means a new entity cannot forget.
///
/// The clock is injected, so these assert an exact instant rather than "roughly now".
/// </remarks>
[Collection(PersistenceCollection.Name)]
public class AuditingInterceptorTests(PostgresFixture fixture) : IAsyncLifetime
{
    private static readonly DateTimeOffset CreatedInstant = new(2026, 3, 1, 9, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset UpdatedInstant = new(2026, 3, 2, 17, 45, 0, TimeSpan.Zero);

    public async ValueTask InitializeAsync()
    {
        await fixture.ResetDatabaseAsync();
        fixture.Clock.Set(CreatedInstant);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Insert_Should_StampCreatedAtFromTheInjectedClock()
    {
        Guid contactId;

        await using (var arrange = fixture.CreateContext())
        {
            var contact = Given.Contact("created@test.local");
            arrange.Add(contact);
            await arrange.SaveChangesAsync();
            contactId = contact.Id;
        }

        await using var assert = fixture.CreateContext();
        var stored = await assert.Contacts.SingleAsync(contact => contact.Id == contactId);

        stored.CreatedAt.ShouldBe(CreatedInstant.UtcDateTime);
        stored.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public async Task Update_Should_StampUpdatedAt_AndLeaveCreatedAtAlone()
    {
        Guid contactId;

        await using (var arrange = fixture.CreateContext())
        {
            var contact = Given.Contact("updated@test.local");
            arrange.Add(contact);
            await arrange.SaveChangesAsync();
            contactId = contact.Id;
        }

        fixture.Clock.Set(UpdatedInstant);

        await using (var act = fixture.CreateContext())
        {
            var contact = await act.Contacts.SingleAsync(row => row.Id == contactId);
            contact.Subject = "Değişti";
            await act.SaveChangesAsync();
        }

        await using var assert = fixture.CreateContext();
        var stored = await assert.Contacts.SingleAsync(contact => contact.Id == contactId);

        stored.UpdatedAt.ShouldBe(UpdatedInstant.UtcDateTime);

        // The interceptor explicitly marks CreatedAt unmodified; without that an update would
        // rewrite it and the record would appear to have been created when it was last touched.
        stored.CreatedAt.ShouldBe(CreatedInstant.UtcDateTime);
    }

    /// <summary>
    /// Even when the caller sets CreatedAt by hand, the interceptor wins — otherwise a client-supplied
    /// value would decide when a record claims to have been created.
    /// </summary>
    [Fact]
    public async Task Insert_Should_OverrideACreatedAtSuppliedByTheCaller()
    {
        Guid contactId;

        await using (var arrange = fixture.CreateContext())
        {
            var contact = Given.Contact("forged@test.local");
            contact.CreatedAt = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            arrange.Add(contact);
            await arrange.SaveChangesAsync();
            contactId = contact.Id;
        }

        await using var assert = fixture.CreateContext();

        (await assert.Contacts.SingleAsync(contact => contact.Id == contactId))
            .CreatedAt.ShouldBe(CreatedInstant.UtcDateTime);
    }

    [Fact]
    public async Task SoftDelete_Should_StampDeletedAtFromTheSameClock()
    {
        Guid seekerId;

        await using (var arrange = fixture.CreateContext())
        {
            var seeker = Given.JobSeeker("deleted-at@test.local");
            arrange.Add(seeker);
            await arrange.SaveChangesAsync();
            seekerId = seeker.Id;
        }

        fixture.Clock.Set(UpdatedInstant);

        await using (var act = fixture.CreateContext())
        {
            act.Remove(await act.JobSeekers.SingleAsync(seeker => seeker.Id == seekerId));
            await act.SaveChangesAsync();
        }

        await using var assert = fixture.CreateContext();
        var stored = await assert.JobSeekers.IgnoreQueryFilters().SingleAsync(seeker => seeker.Id == seekerId);

        stored.DeletedAt.ShouldBe(UpdatedInstant.UtcDateTime);
    }

    /// <summary>Child rows saved with their parent are stamped too, not just the aggregate root.</summary>
    [Fact]
    public async Task Insert_Should_StampChildRowsAsWell()
    {
        Guid cvId;

        await using (var arrange = fixture.CreateContext())
        {
            var seeker = Given.JobSeeker("children@test.local");
            arrange.Add(seeker);

            var cv = Given.Cv(seeker.Id);
            arrange.Add(cv);
            await arrange.SaveChangesAsync();
            cvId = cv.Id;
        }

        await using var assert = fixture.CreateContext();
        var education = await assert.Educations.FirstAsync(row => row.CvId == cvId);

        education.CreatedAt.ShouldBe(CreatedInstant.UtcDateTime);
    }

    /// <summary>Timestamps are UTC, which is what the UtcNow/Now mix used to get wrong.</summary>
    [Fact]
    public async Task Timestamps_Should_BeStoredAsUtc()
    {
        await using var context = fixture.CreateContext();

        var contact = Given.Contact("utc@test.local");
        context.Add(contact);
        await context.SaveChangesAsync();

        contact.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
    }
}
