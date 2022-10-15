using Application.Context;
using MongoDB.Driver;
using Persistence.Configurations;

namespace Persistence.Context
{
    public class MongoContext : IMongoContext
    {
        public MongoClient connection { get; }

        public IMongoDatabase database { get; set; }


        public MongoContext()
        {
            this.connection = new MongoClient(Configuration.ConnectionString);
            this.database = this.connection.GetDatabase("humanresource");
        }
    }
}