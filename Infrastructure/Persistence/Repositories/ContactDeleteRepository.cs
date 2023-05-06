using Application.Context;
using Application.Repositories;
using Domain.Entities;

namespace Persistence.Repositories
{
    public class ContactDeleteRepository : DeleteRepository<Contact>, IContactDeleteRepository
    {
        public ContactDeleteRepository(IMongoContext mongoContext) : base(mongoContext)
        {
        }
    }
}