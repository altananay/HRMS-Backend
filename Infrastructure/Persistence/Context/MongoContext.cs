using Application.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Persistence.Context
{
    // TRANSITIONAL — replaced by HrmsDbContext (EF Core / PostgreSQL) in Phase 3.
    //
    // The connection string now arrives through injected IConfiguration instead of the deleted
    // Persistence.Configurations.Configuration static, which built a fresh ConfigurationManager on
    // every property read and located appsettings.json by walking a relative path from the current
    // working directory up into the WebAPI project. That only worked when the process happened to
    // start from a specific build output folder, and its bare `catch` silently fell back to
    // appsettings.Production.json when it didn't.
    public class MongoContext : IMongoContext
    {
        private const string DatabaseName = "humanresource";

        public MongoClient connection { get; }

        public IMongoDatabase database { get; set; }
        public MongoClientSettings settings { get; set; }

        public MongoContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDb");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:MongoDb is not configured. Set it in appsettings.Development.json " +
                    "or via the ConnectionStrings__MongoDb environment variable.");
            }

            this.connection = new MongoClient(connectionString);
            this.database = this.connection.GetDatabase(DatabaseName);
            this.settings = MongoClientSettings.FromConnectionString(connectionString);
        }
    }
}
