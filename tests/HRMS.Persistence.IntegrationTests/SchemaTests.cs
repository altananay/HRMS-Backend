using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;

namespace HRMS.Persistence.IntegrationTests;

/// <summary>
/// The migrations are the schema record, so this asserts they still produce the model the code
/// assumes.
/// </summary>
[Collection(PersistenceCollection.Name)]
public class SchemaTests(PostgresFixture fixture)
{
    /// <summary>
    /// The fixture applies the real migrations to an empty database; reaching a test at all means
    /// that worked. This states it as an assertion so a broken migration reports itself here rather
    /// than as thirty unrelated failures.
    /// </summary>
    [Fact]
    public async Task Migrations_Should_ApplyToAnEmptyDatabase()
    {
        await using var context = fixture.CreateContext();

        var applied = await context.Database.GetAppliedMigrationsAsync();

        applied.ShouldNotBeEmpty();
        (await context.Database.GetPendingMigrationsAsync()).ShouldBeEmpty();
    }

    /// <summary>
    /// Catches the commonest drift there is: an entity or configuration edited without generating a
    /// migration, which passes every unit test and fails on the first real deployment.
    /// </summary>
    [Fact]
    public async Task Model_Should_HaveNoChangesThatAreNotInAMigration()
    {
        await using var context = fixture.CreateContext();

        context.Database.HasPendingModelChanges().ShouldBeFalse(
            "the entity model differs from the last migration — run `dotnet ef migrations add`");
    }

    [Theory]
    [InlineData("users")]
    [InlineData("job_seekers")]
    [InlineData("employers")]
    [InlineData("system_staff")]
    [InlineData("roles")]
    [InlineData("user_roles")]
    [InlineData("refresh_tokens")]
    [InlineData("cvs")]
    [InlineData("cv_educations")]
    [InlineData("cv_job_experiences")]
    [InlineData("cv_languages")]
    [InlineData("cv_projects")]
    [InlineData("cv_files")]
    [InlineData("employer_departments")]
    [InlineData("job_positions")]
    [InlineData("job_advertisements")]
    [InlineData("job_applications")]
    [InlineData("contacts")]
    public async Task Tables_Should_BeNamedInSnakeCase(string table)
    {
        var exists = await ScalarAsync<bool>(
            "select exists (select 1 from information_schema.tables " +
            "where table_schema = 'public' and table_name = @table)",
            ("table", table));

        exists.ShouldBeTrue($"table {table} is missing");
    }

    /// <summary>
    /// citext is what makes email comparison case-insensitive in the database rather than in every
    /// query that happens to remember <c>ToLower()</c>.
    /// </summary>
    [Fact]
    public async Task UserEmail_Should_UseCitext()
    {
        var type = await ScalarAsync<string>(
            "select udt_name from information_schema.columns " +
            "where table_name = 'users' and column_name = 'email'");

        type.ShouldBe("citext");
    }

    /// <summary>Enums are stored as text, so adding a member cannot reinterpret existing rows.</summary>
    [Theory]
    [InlineData("users", "user_type")]
    [InlineData("job_applications", "status")]
    [InlineData("cv_languages", "level")]
    [InlineData("job_advertisements", "job_type")]
    [InlineData("cv_files", "storage_provider")]
    public async Task Enums_Should_BeStoredAsText(string table, string column)
    {
        var type = await ScalarAsync<string>(
            "select data_type from information_schema.columns " +
            "where table_name = @table and column_name = @column",
            ("table", table), ("column", column));

        type.ShouldBe("character varying");
    }

    /// <summary>A GIN index is the difference between "find CVs with skill X" scanning and seeking.</summary>
    [Theory]
    [InlineData("cvs", "skills")]
    [InlineData("job_advertisements", "skills")]
    public async Task SkillArrays_Should_HaveAGinIndex(string table, string column)
    {
        var count = await ScalarAsync<long>(
            "select count(*) from pg_indexes " +
            "where tablename = @table and indexdef like '%USING gin%' and indexdef like '%' || @column || '%'",
            ("table", table), ("column", column));

        count.ShouldBeGreaterThan(0);
    }

    private async Task<T> ScalarAsync<T>(string sql, params (string Name, object Value)[] parameters)
    {
        await using var context = fixture.CreateContext();
        await using var command = context.Database.GetDbConnection().CreateCommand();

        command.CommandText = sql;

        foreach (var (name, value) in parameters)
        {
            command.Parameters.Add(new NpgsqlParameter(name, value));
        }

        await context.Database.OpenConnectionAsync();
        var result = await command.ExecuteScalarAsync();

        return (T)result!;
    }
}
