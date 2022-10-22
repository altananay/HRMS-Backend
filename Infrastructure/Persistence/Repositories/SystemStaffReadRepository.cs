using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class SystemStaffReadRepository : ReadRepository<SystemStaff>, ISystemStaffReadRepository
    {
        public SystemStaffReadRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}