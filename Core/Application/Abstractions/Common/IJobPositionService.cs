using Application.Results;
using Domain.Entities;

namespace Application.Abstractions.Common
{
    public interface IJobPositionService
    {
        IResult Add(JobPosition jobPosition);
        IResult Delete(string id);
        IResult Update(JobPosition jobPosition);
        IDataResult<IQueryable<JobPosition>> GetAll();
        IDataResult<JobPosition> GetById(string id);

    }
}