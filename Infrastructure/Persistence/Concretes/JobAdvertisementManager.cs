using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.Repositories;
using Application.Results;
using Application.Validators;
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
        public IResult Add(JobAdvertisement jobAdvertisement)
        {
            jobAdvertisement.CreatedAt = DateTime.UtcNow;
            _jobAdvertisementWriteRepository.AddAsync(jobAdvertisement);
            return new SuccessResult(Messages.JobAdvertisementAdded);
        }

        [ValidationAspect(typeof(DeleteValidator))]
        public IResult Delete(string id)
        {
            _jobAdvertisementDeleteRepository.Delete(id);
            return new SuccessResult(Messages.JobAdvertisementDeleted);
        }

        public IDataResult<IQueryable<JobAdvertisement>> GetAll()
        {
            return new SuccessDataResult<IQueryable<JobAdvertisement>>(_jobAdvertisementReadRepository.GetAll());

        }

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

        public IResult Update(JobAdvertisement jobAdvertisement)
        {
            _jobAdvertisementWriteRepository.UpdateAsync(jobAdvertisement);
            return new SuccessResult(Messages.JobAdvertisementUpdated);
        }
    }
}