using Application.Results;
using Application.Utilities.Dtos;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IEmployerReadRepository : IReadRepository<Employer>
    {
        GetEmployerDto GetByEmployerIdWithFields(string id);
        IQueryable<GetAllEmployerDto> GetAllEmployer();
    }
}