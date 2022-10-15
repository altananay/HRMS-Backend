using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class JobPositionWriteRepository : WriteRepository<JobPosition>, IJobPositionWriteRepository
    {
        public JobPositionWriteRepository(IMongoContext mongoContext) : base(mongoContext, "jobpositions")
        {
        }
    }
}