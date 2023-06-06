using Application.Dtos;
using Application.Features.JobApplications.Commands;
using Application.Results;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IJobApplicationService
    {
        Task<IResult> Add(CreateJobApplicationCommand jobApplication);
        Task<IResult> Delete(string id);
        Task<IResult> Update(UpdateJobApplicationCommand jobApplication);
        IDataResult<IQueryable<JobApplication>> GetAll();
        IDataResult<IQueryable<JobApplication>> GetAllByEmployerId(string id);
        IDataResult<IQueryable<JobApplication>> GetAllByJobSeekerId(string id);
        IDataResult<JobApplication> GetById(string id);
        IDataResult<GetJobApplicationResultDto> GetResultById(string id);
    }
}