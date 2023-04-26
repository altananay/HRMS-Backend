using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.Features.JobApplications.Commands;
using Application.Repositories;
using Application.Results;
using Application.Validators.Common;
using Application.Validators.JobAdvertisements;
using Application.Validators.JobApplications;
using Domain.Entities;

namespace Persistence.Concretes
{
    public class JobApplicationManager : IJobApplicationService
    {
        IJobApplicationDeleteRepository _jobApplicationDeleteRepository;
        IJobApplicationReadRepository _jobApplicationReadRepository;
        IJobApplicationWriteRepository _jobApplicationWriteRepository;

        public JobApplicationManager(IJobApplicationDeleteRepository jobApplicationDeleteRepository, IJobApplicationReadRepository jobApplicationReadRepository, IJobApplicationWriteRepository jobApplicationWriteRepository)
        {
            _jobApplicationDeleteRepository = jobApplicationDeleteRepository;
            _jobApplicationReadRepository = jobApplicationReadRepository;
            _jobApplicationWriteRepository = jobApplicationWriteRepository;
        }

        [ValidationAspect(typeof(CreateJobApplicationValidator))]
        public async Task<IResult> Add(CreateJobApplicationCommand jobApplication)
        {
            JobApplication jobApp = new JobApplication();
            jobApp.CreatedAt = DateTime.UtcNow;
            jobApp.JobAdvertisementId = jobApplication.JobAdvertisementId;
            jobApp.Description = jobApplication.Description;
            jobApp.JobSeekerId = jobApplication.JobSeekerId;
            jobApp.Result = jobApplication.Result;
            await _jobApplicationWriteRepository.AddAsync(jobApp);
            return new SuccessResult(Messages.JobApplicationMade);
        }


        [ValidationAspect(typeof(ObjectIdValidator))]
        public async Task<IResult> Delete(string id)
        {
            await _jobApplicationDeleteRepository.Delete(id);
            return new SuccessResult(Messages.JobApplicationDeleted);
        }

        public IDataResult<IQueryable<JobApplication>> GetAll()
        {
            return new SuccessDataResult<IQueryable<JobApplication>>(_jobApplicationReadRepository.GetAll());
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<JobApplication> GetById(string id)
        {
            return new SuccessDataResult<JobApplication>(_jobApplicationReadRepository.GetById(id));
        }

        [ValidationAspect(typeof(UpdateJobApplicationValidator))]
        public async Task<IResult> Update(UpdateJobApplicationCommand jobApplication)
        {
            var oldJobApp = GetById(jobApplication.JobApplicationId);


            JobApplication jobApp = new JobApplication();
            jobApp.Id = oldJobApp.Data.Id;
            jobApp.CreatedAt = oldJobApp.Data.CreatedAt;
            jobApp.JobAdvertisementId = oldJobApp.Data.JobAdvertisementId;
            jobApp.Description = jobApplication.Description;
            jobApp.JobSeekerId = oldJobApp.Data.JobSeekerId;
            jobApp.Result = jobApplication.Result;
            jobApp.UpdatedAt = DateTime.UtcNow;
            await _jobApplicationWriteRepository.UpdateAsync(jobApp);
            return new SuccessResult(Messages.JobApplicationUpdated);
        }
    }
}