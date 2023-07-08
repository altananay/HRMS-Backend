using Application.Results;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IJobPositionService
    {
        Task<IResult> Add(JobPosition jobPosition);
        Task<IResult> Delete(string id);
        Task<IResult> Update(JobPosition jobPosition);
        IDataResult<IQueryable<JobPosition>> GetAll();
        IDataResult<JobPosition> GetById(string id);
    }
}