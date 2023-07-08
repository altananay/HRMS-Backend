using Application.Repositories;
using Application.Utilities.Constants;
using Application.Utilities.Exceptions;

namespace Persistence.Rules
{
    public class ContactBusinessRules
    {
        private readonly IContactReadRepository _contactReadRepository;

        public ContactBusinessRules(IContactReadRepository contactReadRepository)
        {
            _contactReadRepository = contactReadRepository;
        }

        public void CheckIfContactExists(string id)
        {
            if (_contactReadRepository.GetById(id) == null)
            {
                throw new BusinessException(Messages.Contact.ContactNotExists);
            }
        }
    }
}