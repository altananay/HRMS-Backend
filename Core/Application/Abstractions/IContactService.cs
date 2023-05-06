using Application.Features.Contacts.Commands;
using Application.Results;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IContactService
    {
        IDataResult<IQueryable<Contact>> GetAll();
        Task<IResult> AddAsync(CreateContactCommand contact);
        Task<IResult> UpdateAsync(UpdateContactCommand contact);
        Task<IResult> DeleteAsync(string id);
        IDataResult<Contact> GetById(string id);

    }
}