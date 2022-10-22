using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class JobAdvertisementWriteRepository : WriteRepository<JobAdvertisement>, IJobAdvertisementWriteRepository
    {
        public JobAdvertisementWriteRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}