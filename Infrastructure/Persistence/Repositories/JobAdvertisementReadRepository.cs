using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class JobAdvertisementReadRepository : ReadRepository<JobAdvertisement>, IJobAdvertisementReadRepository
    {
        public JobAdvertisementReadRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}