using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.Features.JobAdvertisements.Commands;
using Application.Repositories;
using Application.Results;
using Application.Validators.Common;
using Application.Validators.JobAdvertisements;
using Domain.Entities;

namespace Persistence.Concretes
{
    public class JobAdvertisementManager : IJobAdvertisementService
    {
        private readonly IJobAdvertisementDeleteRepository _jobAdvertisementDeleteRepository;
        private readonly IJobAdvertisementReadRepository _jobAdvertisementReadRepository;
        private readonly IJobAdvertisementWriteRepository _jobAdvertisementWriteRepository;

        public JobAdvertisementManager(IJobAdvertisementDeleteRepository jobAdvertisementDeleteRepository, IJobAdvertisementReadRepository jobAdvertisementReadRepository, IJobAdvertisementWriteRepository jobAdvertisementWriteRepository)
        {
            _jobAdvertisementDeleteRepository = jobAdvertisementDeleteRepository;
            _jobAdvertisementReadRepository = jobAdvertisementReadRepository;
            _jobAdvertisementWriteRepository = jobAdvertisementWriteRepository;
        }

        //[SecuredOperation("employer")]
        [ValidationAspect(typeof(CreateJobAdvertisementValidator))]
        public async Task<IResult> Add(CreateJobAdvertisementCommand jobAdvertisement)
        {
            JobAdvertisement jobAdv = new();
            jobAdv.CreatedAt = DateTime.UtcNow;
            jobAdv.City = jobAdvertisement.City;
            jobAdv.Deadline = jobAdvertisement.Deadline;
            jobAdv.Description = jobAdvertisement.Description;
            jobAdv.JobPositionId = jobAdvertisement.JobPositionId;
            jobAdv.OpenPosition = jobAdvertisement.OpenPosition;
            jobAdv.EmployerId = jobAdvertisement.EmployerId;
            jobAdv.MaxSalary = jobAdvertisement.MaxSalary;
            jobAdv.MinSalary = jobAdvertisement.MinSalary;
            jobAdv.JobType = jobAdvertisement.JobType;
            jobAdv.Status = true;
            
            await _jobAdvertisementWriteRepository.AddAsync(jobAdv);
            return new SuccessResult(Messages.JobAdvertisementAdded);
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public async Task<IResult> Delete(string id)
        {
            await _jobAdvertisementDeleteRepository.Delete(id);
            return new SuccessResult(Messages.JobAdvertisementDeleted);
        }

        public IDataResult<IQueryable<JobAdvertisement>> GetAll()
        {
            return new SuccessDataResult<IQueryable<JobAdvertisement>>(_jobAdvertisementReadRepository.GetAll());

        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<JobAdvertisement> GetById(string id)
        {
            return new SuccessDataResult<JobAdvertisement>(_jobAdvertisementReadRepository.GetById(id));
        }

        public IResult JobAdvertisementExists(string id)
        {
            var result = _jobAdvertisementReadRepository.Get(e => e.Id == id);
            if (result == null)
            {
                return new SuccessDataResult<JobAdvertisement>();
            }
            else
            {
                return new ErrorDataResult<JobAdvertisement>(Messages.JobAdvertisementExists);
            }
        }

        [ValidationAspect(typeof(UpdateJobAdvertisementValidator))]
        public async Task<IResult> Update(UpdateJobAdvertisementCommand jobAdvertisement)
        {
            var oldJobAdv = GetById(jobAdvertisement.Id);

            JobAdvertisement jobAdv = new();
            jobAdv.Id = jobAdvertisement.Id;
            jobAdv.CreatedAt = oldJobAdv.Data.CreatedAt;
            jobAdv.City = jobAdvertisement.City;
            jobAdv.Status = jobAdvertisement.Status;
            jobAdv.Deadline = jobAdvertisement.Deadline;
            jobAdv.Description = jobAdvertisement.Description;
            jobAdv.JobPositionId = jobAdvertisement.JobPositionId;
            jobAdv.OpenPosition = jobAdvertisement.OpenPosition;
            jobAdv.EmployerId = oldJobAdv.Data.EmployerId;
            jobAdv.MaxSalary = jobAdvertisement.MaxSalary;
            jobAdv.MinSalary = jobAdvertisement.MinSalary;
            jobAdv.JobType = jobAdvertisement.JobType;
            jobAdv.UpdatedAt = DateTime.UtcNow;

            await _jobAdvertisementWriteRepository.UpdateAsync(jobAdv);
            return new SuccessResult(Messages.JobAdvertisementUpdated);
        }
    }
}