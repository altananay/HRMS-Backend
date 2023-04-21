using Application.Context;
using Application.Dtos;
using Application.Repositories;
using Domain.Entities;
using MongoDB.Driver;

namespace Persistence.Repositories
{
    public class JobSeekerReadRepository : ReadRepository<JobSeeker>, IJobSeekerReadRepository
    {
        private readonly IMongoContext _mongoContext;
        private readonly string _collection;

        public JobSeekerReadRepository(IMongoContext mongoContext) : base(mongoContext)
        {
            _mongoContext = mongoContext;
            _collection = typeof(JobSeeker).Name.ToLowerInvariant() + "s";
        }

        public IMongoCollection<JobSeeker> collection => _mongoContext.database.GetCollection<JobSeeker>(_collection);

        public IQueryable<GetAllJobSeekerDto> GetAllJobSeeker()
        {
            var result = from jobseeker in collection.AsQueryable()
                         select new GetAllJobSeekerDto { Id= jobseeker.Id, Cv = jobseeker.Cv, FirstName = jobseeker.FirstName, DateOfBirth = jobseeker.DateOfBirth, Email = jobseeker.Email, LastName = jobseeker.LastName, NationalityId = jobseeker.NationalityId };
            return result;
        }
    }
}