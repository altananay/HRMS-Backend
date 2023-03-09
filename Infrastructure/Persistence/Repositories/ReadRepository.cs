using Application.Context;
using Application.Repositories;
using Domain.Common;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Persistence.Repositories
{
    public class ReadRepository<T> : IReadRepository<T> where T : BaseEntity
    {
        private readonly IMongoContext _mongoContext;
        private readonly string _collection;

        public ReadRepository(IMongoContext mongoContext)
        {
            _mongoContext = mongoContext;
            _collection = typeof(T).Name.ToLowerInvariant() + "s";
        }

        public IMongoCollection<T> collection => _mongoContext.database.GetCollection<T>(_collection);

        public T Get(Expression<Func<T, bool>> filter)
        {
            var result = collection.AsQueryable().SingleOrDefault(filter);
            return result;
        }

        public IQueryable<T> GetAll(Expression<Func<T, bool>> filter = null)
        {
            return filter == null ? collection.AsQueryable() : collection.AsQueryable().Where(filter);
        }

        public T GetById(string id)
        {
            var result = collection.AsQueryable().Where(x => x.Id == id.ToString()).FirstOrDefault();
            return result;
        }
    }
}