using Application.Features.JobApplications.Commands;
using Application.Features.JobPositions.Commands;
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
        IDataResult<JobApplication> GetById(string id);
    }
}