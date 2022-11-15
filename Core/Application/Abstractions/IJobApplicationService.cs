using Application.Results;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IJobApplicationService
    {
        IResult Add(JobApplication jobApplication);
        IResult Delete(string id);
        IResult Update(JobApplication jobApplication);
        IDataResult<IQueryable<JobApplication>> GetAll();
        IDataResult<JobApplication> GetById(string id);
    }
}