using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class EmployerDeleteRepository : DeleteRepository<Employer>, IEmployerDeleteRepository
    {
        public EmployerDeleteRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}