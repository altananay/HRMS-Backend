using Application.Dtos;
using Application.Results;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IEmployerService
    {
        IDataResult<IQueryable<Employer>> GetAll();
        IResult Add(Employer employer);
        IResult Delete(string id);
        IResult Update(EmployerForUpdateDto employer);
        IDataResult<Employer> GetById(string id);
        IDataResult<Employer> GetByEmail(string email);
    }
}