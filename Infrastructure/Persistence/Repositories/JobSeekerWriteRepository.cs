using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class JobSeekerWriteRepository : WriteRepository<JobSeeker>, IJobSeekerWriteRepository
    {
        public JobSeekerWriteRepository(IMongoContext mongoContext) : base(mongoContext, "jobseekers")
        {
        }
    }
}