using Application.Context;
using Application.Repositories;
using Domain.Entities;
using MongoDB.Driver;

namespace Persistence.Repositories
{
    public class JobAdvertisementReadRepository : ReadRepository<JobAdvertisement>, IJobAdvertisementReadRepository
    {
        public JobAdvertisementReadRepository(IMongoContext mongoContext) : base(mongoContext)
        {

        }

    }
}