using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.CrossCuttingConcerns.Validation.Validators.Common;
using Application.CrossCuttingConcerns.Validation.Validators.JobApplications;
using Application.Dtos;
using Application.Features.JobApplications.Commands;
using Application.Repositories;
using Application.Results;
using Domain.Entities;

namespace Persistence.Concretes
{
    public class JobApplicationManager : IJobApplicationService
    {
        IJobApplicationDeleteRepository _jobApplicationDeleteRepository;
        IJobApplicationReadRepository _jobApplicationReadRepository;
        IJobApplicationWriteRepository _jobApplicationWriteRepository;
        IJobAdvertisementService _jobAdvertisementService;

        public JobApplicationManager(IJobApplicationDeleteRepository jobApplicationDeleteRepository, IJobApplicationReadRepository jobApplicationReadRepository, IJobApplicationWriteRepository jobApplicationWriteRepository, IJobAdvertisementService jobAdvertisementService)
        {
            _jobApplicationDeleteRepository = jobApplicationDeleteRepository;
            _jobApplicationReadRepository = jobApplicationReadRepository;
            _jobApplicationWriteRepository = jobApplicationWriteRepository;
            _jobAdvertisementService = jobAdvertisementService;
        }

        [ValidationAspect(typeof(CreateJobApplicationValidator))]
        public async Task<IResult> Add(CreateJobApplicationCommand jobApplication)
        {
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
                return new SuccessResult(Messages.JobApplicationMade);
            }
            else
            {
                return new ErrorResult("İş ilanı bulunamadı");
            }
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

        public IDataResult<GetJobApplicationResultDto> GetResultById(string id)
        {
            return new SuccessDataResult<GetJobApplicationResultDto>(_jobApplicationReadRepository.GetResultById(id));
        }

        [ValidationAspect(typeof(UpdateJobApplicationValidator))]
        public async Task<IResult> Update(UpdateJobApplicationCommand jobApplication)
        {
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
            return new SuccessResult(Messages.JobApplicationUpdated);
        }
    }
}