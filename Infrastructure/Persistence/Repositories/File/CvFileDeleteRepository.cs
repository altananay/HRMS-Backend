using Application.Context;
using Application.Repositories.CvFiles;
using Domain.Entities;

namespace Persistence.Repositories.File
{
    public class CvFileDeleteRepository : DeleteRepository<CvFile>, ICvFileDeleteRepository
    {
        public CvFileDeleteRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}