using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.CrossCuttingConcerns.Validation.Validators.Common;
using Application.CrossCuttingConcerns.Validation.Validators.Contacts;
using Application.Features.Contacts.Commands;
using Application.Repositories;
using Application.Results;
using Domain.Entities;

namespace Persistence.Concretes
{
    public class ContactManager : IContactService
    {
        private readonly IContactWriteRepository _contactWriteRepository;
        private readonly IContactReadRepository _contactReadRepository;
        private readonly IContactDeleteRepository _contactDeleteRepository;

        public ContactManager(IContactWriteRepository contactWriteRepository, IContactReadRepository contactReadRepository, IContactDeleteRepository contactDeleteRepository)
        {
            _contactWriteRepository = contactWriteRepository;
            _contactReadRepository = contactReadRepository;
            _contactDeleteRepository = contactDeleteRepository;
        }

        [ValidationAspect(typeof(ContactValidator))]
        public async Task<IResult> AddAsync(CreateContactCommand requestContact)
        {
            Contact contact = new();
            contact.FirstName = requestContact.FirstName;
            contact.LastName = requestContact.LastName;
            contact.Email = requestContact.Email;
            contact.Subject = requestContact.Subject;
            contact.Message = requestContact.Message;
            contact.CreatedAt = DateTime.UtcNow;
            await _contactWriteRepository.AddAsync(contact);
            return new SuccessResult(Messages.ContactAdded);
        }

        [SecuredOperation("admin")]
        [ValidationAspect(typeof(ObjectIdValidator))]
        public async Task<IResult> DeleteAsync(string id)
        {
            await _contactDeleteRepository.Delete(id);
            return new SuccessResult(Messages.ContactDeleted);
        }

        [SecuredOperation("admin")]
        public IDataResult<IQueryable<Contact>> GetAll()
        {
            return new SuccessDataResult<IQueryable<Contact>>(_contactReadRepository.GetAll());
        }

        [SecuredOperation("admin")]
        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<Contact> GetById(string id)
        {
            return new SuccessDataResult<Contact>(_contactReadRepository.GetById(id));
        }

        [SecuredOperation("admin")]
        [ValidationAspect(typeof(UpdateContactValidator))]
        public async Task<IResult> UpdateAsync(UpdateContactCommand requestContact)
        {
            Contact contact = _contactReadRepository.GetById(requestContact.Id);
            contact.FirstName = requestContact.FirstName;
            contact.LastName = requestContact.LastName;
            contact.Email = requestContact.Email;
            contact.Subject = requestContact.Subject;
            contact.Message = requestContact.Message;
            contact.UpdatedAt = DateTime.UtcNow;
            await _contactWriteRepository.UpdateAsync(contact);
            return new SuccessResult(Messages.ContactUpdated);
        }
    }
}