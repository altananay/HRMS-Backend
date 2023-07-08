using Application.Results;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IUserService
    {
        IDataResult<IQueryable<User>> GetAll();
        Task<IResult> Add(User user);
        Task<IResult> Delete(string id);
        Task<IResult> Update(User user);
        IDataResult<User> GetById(string id);
    }
}