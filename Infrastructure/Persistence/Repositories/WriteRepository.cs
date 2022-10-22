using Application.Context;
using Application.Repositories;
using Domain.Common;
using MongoDB.Driver;

namespace Persistence.Repositories
{
    public class WriteRepository<T> : IWriteRepository<T> where T : BaseEntity
    {
        private readonly IMongoContext _mongoContext;
        private readonly string _collection;

        public WriteRepository(IMongoContext mongoContext)
        {
            _mongoContext = mongoContext;
            _collection = typeof(T).Name.ToLowerInvariant() + "s";
        }

        public IMongoCollection<T> collection => _mongoContext.database.GetCollection<T>(_collection);

        public async Task<bool> Add(T entity)
        {
            await collection.InsertOneAsync(entity);
            return true;
        }

        public async Task<bool> Update(T entity)
        {
            var filter = Builders<T>.Filter.Eq(t => t.Id, entity.Id);
            await collection.ReplaceOneAsync(filter, entity);
            return true;
        }
    }
}