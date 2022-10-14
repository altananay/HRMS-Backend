using Application.Context;
using Application.Repositories;
using Domain.Common;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class WriteRepository<T> : IWriteRepository<T> where T : BaseEntity
    {
        private readonly IMongoContext _mongoContext;
        private readonly string _collection;

        public WriteRepository(IMongoContext mongoContext, string collection)
        {
            _mongoContext = mongoContext;
            _collection = collection;
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