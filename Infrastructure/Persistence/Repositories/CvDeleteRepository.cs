using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class CvDeleteRepository : DeleteRepository<Cv>, ICvDeleteRepository
    {
        public CvDeleteRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}