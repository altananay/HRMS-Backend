using Application.Abstractions;
using Application.Aspects;
using Application.CrossCuttingConcerns.Validation.Validators.Common;
using Application.CrossCuttingConcerns.Validation.Validators.JobApplications;
using Application.Features.JobApplications.Commands;
using Application.Repositories;
using Application.Results;
using Application.Utilities.Constants;
using Application.Utilities.Dtos;
using Domain.Entities;
using Persistence.Rules;

namespace Persistence.Concretes
{
    public class JobApplicationManager : IJobApplicationService
    {
        IJobApplicationDeleteRepository _jobApplicationDeleteRepository;
        IJobApplicationReadRepository _jobApplicationReadRepository;
        IJobApplicationWriteRepository _jobApplicationWriteRepository;
        IJobAdvertisementService _jobAdvertisementService;
        JobApplicationBusinessRules _jobApplicationBusinessRules;

        public JobApplicationManager(IJobApplicationDeleteRepository jobApplicationDeleteRepository, IJobApplicationReadRepository jobApplicationReadRepository, IJobApplicationWriteRepository jobApplicationWriteRepository, IJobAdvertisementService jobAdvertisementService, JobApplicationBusinessRules jobApplicationBusinessRules)
        {
            _jobApplicationDeleteRepository = jobApplicationDeleteRepository;
            _jobApplicationReadRepository = jobApplicationReadRepository;
            _jobApplicationWriteRepository = jobApplicationWriteRepository;
            _jobAdvertisementService = jobAdvertisementService;
            _jobApplicationBusinessRules = jobApplicationBusinessRules;
        }

        [ValidationAspect(typeof(CreateJobApplicationValidator))]
        public async Task<IResult> Add(CreateJobApplicationCommand jobApplication)
        {
            _jobApplicationBusinessRules.CheckIfJobAdvertisementExists(jobApplication.JobAdvertisementId);
            _jobApplicationBusinessRules.CheckIfJobSeekerExists(jobApplication.JobSeekerId);
            var result = _jobAdvertisementService.GetById(jobApplication.JobAdvertisementId);
            if (result.IsSuccess)
            {
                JobApplication jobApp = new JobApplication();
                jobApp.CreatedAt = DateTime.UtcNow;
                jobApp.JobAdvertisementId = jobApplication.JobAdvertisementId;
                jobApp.EmployerId = result.Data.EmployerId;
                jobApp.JobSeekerDescription = jobApplication.JobSeekerDescription;
                jobApp.JobSeekerId = jobApplication.JobSeekerId;
                jobApp.Result = "İş başvurusu yapıldı.";
                await _jobApplicationWriteRepository.AddAsync(jobApp);
                return new SuccessResult(Messages.JobApplication.JobApplicationMade);
            }
            else
            {
                return new ErrorResult(Messages.JobApplication.JobApplicationNotFound);
            }
        }


        [ValidationAspect(typeof(ObjectIdValidator))]
        public async Task<IResult> Delete(string id)
        {
            _jobApplicationBusinessRules.CheckIfJobApplicationExists(id);
            await _jobApplicationDeleteRepository.Delete(id);
            return new SuccessResult(Messages.JobApplication.JobApplicationDeleted);
        }

        public IDataResult<IQueryable<JobApplication>> GetAll()
        {
            return new SuccessDataResult<IQueryable<JobApplication>>(_jobApplicationReadRepository.GetAll());
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<IQueryable<JobApplication>> GetAllByEmployerId(string id)
        {
            _jobApplicationBusinessRules.CheckIfEmployerExists(id);
            return new SuccessDataResult<IQueryable<JobApplication>>(_jobApplicationReadRepository.GetAll(jobApp => jobApp.EmployerId == id));
        }

        [ValidationAspect(typeof(ObjectIdValidator))]

        public IDataResult<IQueryable<JobApplication>> GetAllByJobSeekerId(string id)
        {
            _jobApplicationBusinessRules.CheckIfJobSeekerExists(id);
            return new SuccessDataResult<IQueryable<JobApplication>>(_jobApplicationReadRepository.GetAll(jobApp => jobApp.JobSeekerId == id));
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<JobApplication> GetById(string id)
        {
            _jobApplicationBusinessRules.CheckIfJobApplicationExists(id);
            return new SuccessDataResult<JobApplication>(_jobApplicationReadRepository.GetById(id));
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<GetJobApplicationResultDto> GetResultById(string id)
        {
            _jobApplicationBusinessRules.CheckIfJobApplicationExists(id);
            return new SuccessDataResult<GetJobApplicationResultDto>(_jobApplicationReadRepository.GetResultById(id));
        }

        [ValidationAspect(typeof(UpdateJobApplicationValidator))]
        public async Task<IResult> Update(UpdateJobApplicationCommand jobApplication)
        {
            _jobApplicationBusinessRules.CheckIfJobApplicationExists(jobApplication.JobApplicationId);
            var oldJobApp = GetById(jobApplication.JobApplicationId);

            JobApplication jobApp = new JobApplication();
            jobApp.Id = oldJobApp.Data.Id;
            jobApp.CreatedAt = oldJobApp.Data.CreatedAt;
            jobApp.JobAdvertisementId = oldJobApp.Data.JobAdvertisementId;
            jobApp.JobSeekerDescription = oldJobApp.Data.JobSeekerDescription;
            jobApp.EmployerDescription = jobApplication.EmployerDescription;
            jobApp.JobSeekerId = oldJobApp.Data.JobSeekerId;
            jobApp.EmployerId = oldJobApp.Data.EmployerId;
            jobApp.Result = jobApplication.Result;
            jobApp.UpdatedAt = DateTime.UtcNow;
            await _jobApplicationWriteRepository.UpdateAsync(jobApp);
            return new SuccessResult(Messages.JobApplication.JobApplicationUpdated);
        }
    }
}