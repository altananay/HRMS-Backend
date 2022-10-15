using Application.Results;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IJobSeekerService
    {
        IDataResult<IQueryable<JobSeeker>> GetAll();
        IResult Add(JobSeeker user);
        IResult Delete(string id);
        IResult Update(JobSeeker user);
        //IDataResult<IQueryable<JobSeeker>> GetClaims(JobSeeker user);
        IDataResult<JobSeeker> GetByMail(string email);
    }
}