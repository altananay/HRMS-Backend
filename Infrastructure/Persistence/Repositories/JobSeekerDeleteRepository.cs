using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class JobSeekerDeleteRepository : DeleteRepository<JobSeeker>, IJobSeekerDeleteRepository
    {
        public JobSeekerDeleteRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}