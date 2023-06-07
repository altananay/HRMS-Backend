using Application.Dtos;
using Application.Features.JobSeekers.Commands;
using Application.Results;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IJobSeekerService
    {
        IDataResult<IQueryable<JobSeeker>> GetAll();
        Task<IResult> Add(JobSeeker jobSeeker);
        Task<IResult> Delete(string id);
        Task<IResult> Update(UpdateJobSeekerCommand jobSeeker);
        //IDataResult<IQueryable<JobSeeker>> GetClaims(JobSeeker user);
        IDataResult<JobSeeker> GetByMail(string email);
        IDataResult<JobSeeker> GetById(string id);
        IResult NationalityIdExists(string nationalityId);
        IDataResult<IQueryable<GetAllJobSeekerDto>> GetAllJobSeeker();
    }
}