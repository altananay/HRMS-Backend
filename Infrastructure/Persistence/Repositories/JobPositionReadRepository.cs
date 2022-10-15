using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class JobPositionReadRepository : ReadRepository<JobPosition>, IJobPositionReadRepository
    {
        public JobPositionReadRepository(IMongoContext mongoContext) : base(mongoContext, "jobpositions")
        {
        }
    }
}