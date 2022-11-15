using Application.Dtos;
using Application.Results;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IUserService
    {
        IDataResult<IQueryable<User>> GetAll();
        IResult Add(User user);
        IResult Delete(string id);
        IResult Update(User user);
        IDataResult<User> GetById(string id);
    }
}