using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class UserDeleteRepository : DeleteRepository<User>, IUserDeleteRepository
    {
        public UserDeleteRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}