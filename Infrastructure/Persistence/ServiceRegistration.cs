using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using Persistence.Configurations;

namespace Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static void AddPersistenceServices<T>(this IServiceCollection services, ILogger<T> logger)
        {
            var database = Configuration.Database;
            
            services.AddSingleton<IMongoClient>(configuration =>
            {
                var mongoClientSettings = MongoClientSettings.FromConnectionString(Configuration.ConnectionString);

                mongoClientSettings.ClusterConfigurator = clusterBuilder =>
                {
                    clusterBuilder.Subscribe<CommandStartedEvent>(e =>
                    {
                        logger.LogInformation($"Command started: {e.CommandName} - JSON: {e.Command.ToJson()}");
                    });

                    clusterBuilder.Subscribe<CommandSucceededEvent>(e =>
                    {
                        logger.LogInformation($"Command succeed: {e.CommandName}");
                    });
                };

                return new MongoClient(mongoClientSettings);
            });
        }
    }
}