using Application.Features.Employers.Commands;
using Application.Results;
using Application.Utilities.Dtos;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IEmployerService
    {
        IDataResult<IQueryable<Employer>> GetAll();
        Task<IResult> Add(Employer employer);
        Task<IResult> Delete(string id);
        Task<IResult> Update(UpdateEmployerCommand employer);
        IDataResult<Employer> GetById(string id);
        IDataResult<Employer> GetByEmail(string email);
        IDataResult<GetEmployerDto> GetByEmployerIdWithFields(string id);
        IDataResult<IQueryable<GetAllEmployerDto>> GetAllEmployer();
        IDataResult<IQueryable<Employer>> GetAllByHighestNumberOfEmployees(); 
    }
}