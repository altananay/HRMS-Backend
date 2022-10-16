using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class EmployerReadRepository : ReadRepository<Employer>, IEmployerReadRepository
    {
        public EmployerReadRepository(IMongoContext mongoContext) : base(mongoContext, "employers")
        {
        }
    }
}