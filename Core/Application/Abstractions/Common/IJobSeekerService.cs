using Application.Dtos;
using Application.Results;
using Domain.Entities;

namespace Application.Abstractions.Common
{
    public interface IJobSeekerService
    {
        IDataResult<IQueryable<JobSeeker>> GetAll();
        IResult Add(JobSeeker jobSeeker);
        IResult Delete(string id);
        IResult Update(JobSeeker user);
        //IDataResult<IQueryable<JobSeeker>> GetClaims(JobSeeker user);
        IDataResult<JobSeeker> GetByMail(string email);
    }
}