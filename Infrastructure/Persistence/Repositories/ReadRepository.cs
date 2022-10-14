using Application.Context;
using Application.Repositories;
using Domain.Common;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class ReadRepository<T> : IReadRepository<T> where T : BaseEntity
    {
        private readonly IMongoContext _mongoContext;
        private readonly string _collection;

        public ReadRepository(IMongoContext mongoContext, string collection)
        {
            _mongoContext = mongoContext;
            _collection = collection;
        }

        public IMongoCollection<T> collection => _mongoContext.database.GetCollection<T>(_collection);

        public T Get(Expression<Func<T, bool>> filter)
        {
            return collection.AsQueryable().SingleOrDefault(filter);
        }

        public IQueryable<T> GetAll()
        {
            var results = collection.AsQueryable();
            return results;
        }

        public T GetById(string id)
        {
            var result = collection.AsQueryable().Where(x => x.Id == id.ToString()).FirstOrDefault();
            return result;
        }
    }
}