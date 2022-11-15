using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    internal class JobApplicationReadRepository : ReadRepository<JobApplication>, IJobApplicationReadRepository
    {
        public JobApplicationReadRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}