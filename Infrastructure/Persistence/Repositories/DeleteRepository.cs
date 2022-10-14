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
    public class DeleteRepository<T> : IDeleteRepository<T> where T : BaseEntity
    {
        private readonly IMongoContext _mongoContext;
        private readonly string _collection;

        public DeleteRepository(IMongoContext mongoContext, string collection)
        {
            _mongoContext = mongoContext;
            _collection = collection;
        }

        public IMongoCollection<T> collection => _mongoContext.database.GetCollection<T>(_collection);

        public async Task<bool> Delete(T entity)
        {
            await collection.DeleteOneAsync(t => t.Id == entity.Id);
            return true;
        }
    }
}
