using Application.Abstractions;
using Application.Aspects;
using Application.CrossCuttingConcerns.Validation.Validators.Common;
using Application.CrossCuttingConcerns.Validation.Validators.Contacts;
using Application.Features.Contacts.Commands;
using Application.Repositories;
using Application.Results;
using Application.Utilities.Constants;
using AutoMapper;
using Domain.Entities;
using Persistence.Rules;

namespace Persistence.Concretes
{
    public class ContactManager : IContactService
    {
        private readonly IContactWriteRepository _contactWriteRepository;
        private readonly IContactReadRepository _contactReadRepository;
        private readonly IContactDeleteRepository _contactDeleteRepository;
        private readonly ContactBusinessRules _rules;
        private readonly IMapper _mapper;

        public ContactManager(IContactWriteRepository contactWriteRepository, IContactReadRepository contactReadRepository, IContactDeleteRepository contactDeleteRepository, ContactBusinessRules rules, IMapper mapper)
        {
            _contactWriteRepository = contactWriteRepository;
            _contactReadRepository = contactReadRepository;
            _contactDeleteRepository = contactDeleteRepository;
            _rules = rules;
            _mapper = mapper;
        }

        [ValidationAspect(typeof(ContactValidator))]
        public async Task<IResult> AddAsync(CreateContactCommand requestContact)
        {
            Contact contact = _mapper.Map<Contact>(requestContact);
            contact.CreatedAt = DateTime.UtcNow;
            await _contactWriteRepository.AddAsync(contact);
            return new SuccessResult(Messages.Contact.ContactAdded);
        }

        [SecuredOperation("admin")]
        [ValidationAspect(typeof(ObjectIdValidator))]
        public async Task<IResult> DeleteAsync(string id)
        {
            _rules.CheckIfContactExists(id);
            await _contactDeleteRepository.Delete(id);
            return new SuccessResult(Messages.Contact.ContactDeleted);
        }
        
        //[SecuredOperation("admin")]
        public IDataResult<IQueryable<Contact>> GetAll()
        {
            return new SuccessDataResult<IQueryable<Contact>>(_contactReadRepository.GetAll());
        }

        [SecuredOperation("admin")]
        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<Contact> GetById(string id)
        {
            _rules.CheckIfContactExists(id);
            return new SuccessDataResult<Contact>(_contactReadRepository.GetById(id));
        }

        //[SecuredOperation("admin")]
        [ValidationAspect(typeof(UpdateContactValidator))]
        public async Task<IResult> UpdateAsync(UpdateContactCommand requestContact)
        {
            _rules.CheckIfContactExists(requestContact.Id);
            Contact contact = _contactReadRepository.GetById(requestContact.Id);
            contact = _mapper.Map<Contact>(requestContact);
            contact.UpdatedAt = DateTime.UtcNow;
            await _contactWriteRepository.UpdateAsync(contact);
            return new SuccessResult(Messages.Contact.ContactUpdated);
        }
    }
}