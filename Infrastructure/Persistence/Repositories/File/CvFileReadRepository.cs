using Application.Context;
using Application.Repositories.CvFiles;
using Domain.Entities;

namespace Persistence.Repositories.File
{
    public class CvFileReadRepository : ReadRepository<CvFile>, ICvFileReadRepository
    {
        public CvFileReadRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}