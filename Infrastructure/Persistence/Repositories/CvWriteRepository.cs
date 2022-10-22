using Application.Context;
using Application.Repositories;
using Domain.Entities;


namespace Persistence.Repositories
{
    public class CvWriteRepository : WriteRepository<Cv>, ICvWriteRepository
    {
        public CvWriteRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}