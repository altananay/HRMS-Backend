using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class JobPositionDeleteRepository : DeleteRepository<JobPosition>, IJobPositionDeleteRepository
    {
        public JobPositionDeleteRepository(IMongoContext mongoContext) : base(mongoContext, "jobpositions")
        {
        }
    }
}