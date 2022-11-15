using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class JobApplicationWriteRepository : WriteRepository<JobApplication>, IJobApplicationWriteRepository
    {
        public JobApplicationWriteRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}