using Application.Features.JobSeekers.Commands;
using Application.Results;
using Application.Utilities.Dtos;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IJobSeekerService
    {
        IDataResult<IQueryable<JobSeeker>> GetAll();
        Task<IResult> Add(JobSeeker jobSeeker);
        Task<IResult> Delete(string id);
        Task<IResult> Update(UpdateJobSeekerCommand jobSeeker);
        Task<IResult> UpdateCvById(string id, Cv cv);
        //IDataResult<IQueryable<JobSeeker>> GetClaims(JobSeeker user);
        IDataResult<JobSeeker> GetByMail(string email);
        IDataResult<JobSeeker> GetById(string id);
        IDataResult<IQueryable<GetAllJobSeekerDto>> GetAllJobSeeker();
    }
}