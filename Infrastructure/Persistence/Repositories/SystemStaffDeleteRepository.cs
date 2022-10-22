using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class SystemStaffDeleteRepository : DeleteRepository<SystemStaff>, ISystemStaffDeleteRepository
    {
        public SystemStaffDeleteRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}