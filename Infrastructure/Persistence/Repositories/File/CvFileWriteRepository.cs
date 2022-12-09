using Application.Context;
using Application.Repositories.CvFiles;
using Domain.Entities;

namespace Persistence.Repositories.File
{
    public class CvFileWriteRepository : WriteRepository<CvFile>, ICvFileWriteRepository
    {
        public CvFileWriteRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}