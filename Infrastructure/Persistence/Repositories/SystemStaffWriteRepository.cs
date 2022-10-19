using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class SystemStaffWriteRepository : WriteRepository<SystemStaff>, ISystemStaffWriteRepository
    {
        public SystemStaffWriteRepository(IMongoContext mongoContext) : base(mongoContext, "systemstaffs")
        {
        }
    }
}