using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class CvReadRepository : ReadRepository<Cv>, ICvReadRepository
    {
        public CvReadRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}