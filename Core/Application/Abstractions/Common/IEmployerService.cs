using Application.Dtos;
using Application.Results;
using Domain.Entities;

namespace Application.Abstractions.Common
{
    public interface IEmployerService
    {
        IDataResult<IQueryable<Employer>> GetAll();
        IResult Add(EmployerForRegisterDto employer);
        IResult Delete(string id);
        IResult Update(EmployerForUpdateDto employer);
        IDataResult<Employer> GetById(string id);
        IDataResult<Employer> GetByEmail(string email);
    }
}