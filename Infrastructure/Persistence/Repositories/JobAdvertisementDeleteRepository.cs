using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class JobAdvertisementDeleteRepository : DeleteRepository<JobAdvertisement>, IJobAdvertisementDeleteRepository
    {
        public JobAdvertisementDeleteRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}