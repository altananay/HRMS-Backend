using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class JobSeekerReadRepository : ReadRepository<JobSeeker>, IJobSeekerReadRepository
    {
        public JobSeekerReadRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}