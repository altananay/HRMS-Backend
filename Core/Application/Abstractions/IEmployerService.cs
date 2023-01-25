using Application.Features.Employers.Commands;
using Application.Results;
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
    }
}