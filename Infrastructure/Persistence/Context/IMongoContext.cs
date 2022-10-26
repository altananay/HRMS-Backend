using MongoDB.Driver;

namespace Application.Context
{
    public interface IMongoContext
    {
        MongoClient connection { get; }
        IMongoDatabase database { get; }
    }
}