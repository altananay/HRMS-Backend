using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class JobApplicationDeleteRepository : DeleteRepository<JobApplication>, IJobApplicationDeleteRepository
    {
        public JobApplicationDeleteRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}