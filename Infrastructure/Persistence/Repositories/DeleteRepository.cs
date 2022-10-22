using Application.Context;
using Application.Repositories;
using Domain.Common;
using MongoDB.Driver;

namespace Persistence.Repositories
{
    public class DeleteRepository<T> : IDeleteRepository<T> where T : BaseEntity
    {
        private readonly IMongoContext _mongoContext;
        private readonly string _collection;

        public DeleteRepository(IMongoContext mongoContext)
        {
            _mongoContext = mongoContext;
            _collection = typeof(T).Name.ToLowerInvariant() + "s";
        }

        public IMongoCollection<T> collection => _mongoContext.database.GetCollection<T>(_collection);

        public async Task<bool> Delete(string id)
        {
            await collection.DeleteOneAsync(t => t.Id == id);
            return true;
        }
    }
}