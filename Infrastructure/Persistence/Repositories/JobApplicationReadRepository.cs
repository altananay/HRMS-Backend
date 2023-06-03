using Application.Context;
using Application.Dtos;
using Application.Repositories;
using Domain.Entities;
using MongoDB.Driver;

namespace Persistence.Repositories
{
    public class JobApplicationReadRepository : ReadRepository<JobApplication>, IJobApplicationReadRepository
    {
        private readonly IMongoContext? _mongoContext;
        private readonly string? _collection;

        public JobApplicationReadRepository(IMongoContext mongoContext) : base(mongoContext)
        {
            _mongoContext = mongoContext;
            _collection = typeof(JobApplication).Name.ToLowerInvariant() + "s";
        }

        public IMongoCollection<JobApplication> collection => _mongoContext.database.GetCollection<JobApplication>(_collection);

        public GetJobApplicationResultDto GetResultById(string id)
        {
            var result = (from jobapp in collection.AsQueryable() where jobapp.Id == id select new GetJobApplicationResultDto { Id = jobapp.Id, Result = jobapp.Result }).FirstOrDefault();
            return result;
        }
    }
}