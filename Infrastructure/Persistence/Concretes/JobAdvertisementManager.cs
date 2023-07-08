using Application.Abstractions;
using Application.Aspects;
using Application.CrossCuttingConcerns.Validation.Validators.Common;
using Application.CrossCuttingConcerns.Validation.Validators.JobAdvertisements;
using Application.Features.JobAdvertisements.Commands;
using Application.Repositories;
using Application.Results;
using Application.Utilities.Constants;
using AutoMapper;
using Domain.Entities;
using Persistence.Rules;

namespace Persistence.Concretes
{
    public class JobAdvertisementManager : IJobAdvertisementService
    {
        private readonly IJobAdvertisementDeleteRepository _jobAdvertisementDeleteRepository;
        private readonly IJobAdvertisementReadRepository _jobAdvertisementReadRepository;
        private readonly IJobAdvertisementWriteRepository _jobAdvertisementWriteRepository;
        private readonly IEmployerService _employerService;
        private readonly IJobPositionService _jobPositionService;
        private readonly JobAdvertisementBusinessRules _jobAdvertisementBusinessRules;
        private readonly IMapper _mapper;

        public JobAdvertisementManager(IJobAdvertisementDeleteRepository jobAdvertisementDeleteRepository, IJobAdvertisementReadRepository jobAdvertisementReadRepository, IJobAdvertisementWriteRepository jobAdvertisementWriteRepository, IEmployerService employerService, IJobPositionService jobPositionService, JobAdvertisementBusinessRules jobAdvertisementBusinessRules, IMapper mapper)
        {
            _jobAdvertisementDeleteRepository = jobAdvertisementDeleteRepository;
            _jobAdvertisementReadRepository = jobAdvertisementReadRepository;
            _jobAdvertisementWriteRepository = jobAdvertisementWriteRepository;
            _employerService = employerService;
            _jobPositionService = jobPositionService;
            _jobAdvertisementBusinessRules = jobAdvertisementBusinessRules;
            _mapper = mapper;
        }

        //[SecuredOperation("employer")]
        [ValidationAspect(typeof(CreateJobAdvertisementValidator))]
        public async Task<IResult> Add(CreateJobAdvertisementCommand jobAdvertisement)
        {
            JobPosition jobPosition = new();
            var employer = _employerService.GetById(jobAdvertisement.EmployerId);
            jobPosition.CreatedAt = DateTime.Now;
            jobPosition.PositionName = jobAdvertisement.JobPositionName;

            await _jobPositionService.Add(jobPosition);

            JobAdvertisement jobAdv = _mapper.Map<JobAdvertisement>(jobAdvertisement);
            jobAdv.CreatedAt = DateTime.UtcNow;
            jobAdv.CompanyName = employer.Data.CompanyName;
            jobAdv.CompanyPhone = employer.Data.CompanyPhone;
            jobAdv.WebSite = employer.Data.WebSite;
            jobAdv.EmployerId = employer.Data.Id;
            jobAdv.JobPositionId = jobPosition.Id;
            jobAdv.Email = employer.Data.Email;
            jobAdv.Status = true;      
            await _jobAdvertisementWriteRepository.AddAsync(jobAdv);
            return new SuccessResult(Messages.JobAdvertisement.JobAdvertisementAdded);
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public async Task<IResult> Delete(string id)
        {
            _jobAdvertisementBusinessRules.JobAdvertisementExists(id);
            JobAdvertisement jobAdvertisement = _jobAdvertisementReadRepository.GetById(id);
            await _jobPositionService.Delete(jobAdvertisement.JobPositionId);
            await _jobAdvertisementDeleteRepository.Delete(id);
            return new SuccessResult(Messages.JobAdvertisement.JobAdvertisementDeleted);
        }

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

        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<IQueryable<JobAdvertisement>> GetByEmployerId(string id)
        {
            _jobAdvertisementBusinessRules.JobAdvertisementExists(id);
            return new SuccessDataResult<IQueryable<JobAdvertisement>>(_jobAdvertisementReadRepository.GetAll(j => j.EmployerId == id));
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<IQueryable<JobAdvertisement>> GetByEmployerIdWithStatus(string id, bool status)
        {
            _jobAdvertisementBusinessRules.JobAdvertisementExists(id);
            return new SuccessDataResult<IQueryable<JobAdvertisement>>(_jobAdvertisementReadRepository.GetAll(ja => ja.EmployerId == id && ja.Status == status));
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<JobAdvertisement> GetById(string id)
        {
            _jobAdvertisementBusinessRules.JobAdvertisementExists(id);
            return new SuccessDataResult<JobAdvertisement>(_jobAdvertisementReadRepository.GetById(id));
        }

        

        [ValidationAspect(typeof(UpdateJobAdvertisementValidator))]
        public async Task<IResult> Update(UpdateJobAdvertisementCommand jobAdvertisement)
        {
            _jobAdvertisementBusinessRules.JobAdvertisementExists(jobAdvertisement.Id);
            var oldJobAdv = GetById(jobAdvertisement.Id);
            var jobPosition = _jobPositionService.GetById(jobAdvertisement.JobPositionId);
            jobPosition.Data.PositionName = jobAdvertisement.JobPositionName;
            jobPosition.Data.UpdatedAt = DateTime.Now;

            await _jobPositionService.Update(jobPosition.Data);

            JobAdvertisement jobAdv = _mapper.Map<JobAdvertisement>(jobAdvertisement);

            jobAdv.CreatedAt = oldJobAdv.Data.CreatedAt;
            jobAdv.EmployerId = oldJobAdv.Data.EmployerId;
            jobAdv.JobType = jobAdvertisement.JobType;
            jobAdv.UpdatedAt = DateTime.UtcNow;
            jobAdv.CompanyName = oldJobAdv.Data.CompanyName;
            jobAdv.CompanyPhone = oldJobAdv.Data.CompanyPhone;
            jobAdv.WebSite = oldJobAdv.Data.WebSite;
            jobAdv.Email = oldJobAdv.Data.Email;
            

            await _jobAdvertisementWriteRepository.UpdateAsync(jobAdv);
            return new SuccessResult(Messages.JobAdvertisement.JobAdvertisementUpdated);
        }
    }
}