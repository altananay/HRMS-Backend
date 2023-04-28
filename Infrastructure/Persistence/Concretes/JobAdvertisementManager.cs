using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.CrossCuttingConcerns.Validation.Validators.Common;
using Application.CrossCuttingConcerns.Validation.Validators.JobAdvertisements;
using Application.Features.JobAdvertisements.Commands;
using Application.Repositories;
using Application.Results;
using Domain.Entities;

namespace Persistence.Concretes
{
    public class JobAdvertisementManager : IJobAdvertisementService
    {
        private readonly IJobAdvertisementDeleteRepository _jobAdvertisementDeleteRepository;
        private readonly IJobAdvertisementReadRepository _jobAdvertisementReadRepository;
        private readonly IJobAdvertisementWriteRepository _jobAdvertisementWriteRepository;
        private readonly IEmployerReadRepository _employerReadRepository;
        private readonly IJobPositionWriteRepository _jobPositionWriteRepository;
        private readonly IJobPositionReadRepository _jobPositionReadRepository;
        private readonly IJobPositionDeleteRepository _jobPositionDeleteRepository;

        public JobAdvertisementManager(IJobAdvertisementDeleteRepository jobAdvertisementDeleteRepository, IJobAdvertisementReadRepository jobAdvertisementReadRepository, IJobAdvertisementWriteRepository jobAdvertisementWriteRepository, IEmployerReadRepository employerReadRepository, IJobPositionWriteRepository jobPositionWriteRepository, IJobPositionReadRepository jobPositionReadRepository, IJobPositionDeleteRepository jobPositionDeleteRepository)
        {
            _jobAdvertisementDeleteRepository = jobAdvertisementDeleteRepository;
            _jobAdvertisementReadRepository = jobAdvertisementReadRepository;
            _jobAdvertisementWriteRepository = jobAdvertisementWriteRepository;
            _employerReadRepository = employerReadRepository;
            _jobPositionWriteRepository = jobPositionWriteRepository;
            _jobPositionReadRepository = jobPositionReadRepository;
            _jobPositionDeleteRepository = jobPositionDeleteRepository;
        }

        //[SecuredOperation("employer")]
        [ValidationAspect(typeof(CreateJobAdvertisementValidator))]
        public async Task<IResult> Add(CreateJobAdvertisementCommand jobAdvertisement)
        {
            JobPosition jobPosition = new();
            Employer employer = _employerReadRepository.GetById(jobAdvertisement.EmployerId);
            jobPosition.CreatedAt = DateTime.Now;
            jobPosition.PositionName = jobAdvertisement.JobPositionName;

            await _jobPositionWriteRepository.AddAsync(jobPosition);

            JobAdvertisement jobAdv = new();
            jobAdv.CreatedAt = DateTime.UtcNow;
            jobAdv.City = jobAdvertisement.City;
            jobAdv.Deadline = jobAdvertisement.Deadline;
            jobAdv.Description = jobAdvertisement.Description;
            jobAdv.JobPosition = jobAdvertisement.JobPositionName;
            jobAdv.OpenPosition = jobAdvertisement.OpenPosition;
            jobAdv.CompanyName = employer.CompanyName;
            jobAdv.CompanyPhone = employer.CompanyPhone;
            jobAdv.WebSite = employer.WebSite;
            jobAdv.EmployerId = employer.Id;
            jobAdv.JobPositionId = jobPosition.Id;
            jobAdv.Skills = jobAdvertisement.Skills;
            jobAdv.Experience = jobAdvertisement.Experience;
            jobAdv.Title = jobAdvertisement.Title;
            jobAdv.Email = employer.Email;
            jobAdv.MaxSalary = jobAdvertisement.MaxSalary;
            jobAdv.MinSalary = jobAdvertisement.MinSalary;
            jobAdv.JobType = jobAdvertisement.JobType;
            jobAdv.Status = true;
            jobAdv.Currency = jobAdvertisement.Currency;
            
            
            await _jobAdvertisementWriteRepository.AddAsync(jobAdv);
            return new SuccessResult(Messages.JobAdvertisementAdded);
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public async Task<IResult> Delete(string id)
        {
            JobAdvertisement jobAdvertisement = _jobAdvertisementReadRepository.GetById(id);
            await _jobPositionDeleteRepository.Delete(jobAdvertisement.JobPositionId);
            await _jobAdvertisementDeleteRepository.Delete(id);
            return new SuccessResult(Messages.JobAdvertisementDeleted);
        }

        [SecuredOperation("employer")]
        public IDataResult<IQueryable<JobAdvertisement>> GetAll()
        {
            return new SuccessDataResult<IQueryable<JobAdvertisement>>(_jobAdvertisementReadRepository.GetAll());
        }

        public IDataResult<IQueryable<JobAdvertisement>> GetAllByHighestSalary()
        {
            return new SuccessDataResult<IQueryable<JobAdvertisement>>(_jobAdvertisementReadRepository.GetAll().OrderByDescending(j=> j.MaxSalary));
        }

        public IDataResult<IQueryable<JobAdvertisement>> GetAllByStatus(bool status)
        {
            return new SuccessDataResult<IQueryable<JobAdvertisement>>(_jobAdvertisementReadRepository.GetAll(j => j.Status == status));
        }

        public IDataResult<IQueryable<JobAdvertisement>> GetByEmployerId(string id)
        {
            return new SuccessDataResult<IQueryable<JobAdvertisement>>(_jobAdvertisementReadRepository.GetAll(j => j.EmployerId == id));
        }

        public IDataResult<IQueryable<JobAdvertisement>> GetByEmployerIdWithStatus(string id, bool status)
        {
            return new SuccessDataResult<IQueryable<JobAdvertisement>>(_jobAdvertisementReadRepository.GetAll(ja => ja.EmployerId == id && ja.Status == status));
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
            JobPosition jobPosition = _jobPositionReadRepository.GetById(jobAdvertisement.JobPositionId);
            jobPosition.PositionName = jobAdvertisement.JobPositionName;
            jobPosition.UpdatedAt = DateTime.Now;

            await _jobPositionWriteRepository.UpdateAsync(jobPosition);

            JobAdvertisement jobAdv = new();

            jobAdv.Id = jobAdvertisement.Id;
            jobAdv.CreatedAt = oldJobAdv.Data.CreatedAt;
            jobAdv.City = jobAdvertisement.City;
            jobAdv.Status = jobAdvertisement.Status;
            jobAdv.Deadline = jobAdvertisement.Deadline;
            jobAdv.Description = jobAdvertisement.Description;
            jobAdv.Experience = jobAdvertisement.Experience;
            jobAdv.Title = jobAdvertisement.Title;
            jobAdv.JobPosition = jobAdvertisement.JobPositionName;
            jobAdv.JobPositionId = jobAdvertisement.JobPositionId;
            jobAdv.OpenPosition = jobAdvertisement.OpenPosition;
            jobAdv.EmployerId = oldJobAdv.Data.EmployerId;
            jobAdv.MaxSalary = jobAdvertisement.MaxSalary;
            jobAdv.MinSalary = jobAdvertisement.MinSalary;
            jobAdv.JobType = jobAdvertisement.JobType;
            jobAdv.UpdatedAt = DateTime.UtcNow;
            jobAdv.Skills = jobAdvertisement.Skills;
            jobAdv.CompanyName = oldJobAdv.Data.CompanyName;
            jobAdv.CompanyPhone = oldJobAdv.Data.CompanyPhone;
            jobAdv.WebSite = oldJobAdv.Data.WebSite;
            jobAdv.Email = oldJobAdv.Data.Email;
            jobAdv.Currency = jobAdvertisement.Currency;
            

            await _jobAdvertisementWriteRepository.UpdateAsync(jobAdv);
            return new SuccessResult(Messages.JobAdvertisementUpdated);
        }
    }
}