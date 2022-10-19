using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.Repositories;
using Application.Results;
using Application.Validators;
using Domain.Entities;

namespace Persistence.Concretes
{
    public class JobPositionManager : IJobPositionService
    {
        IJobPositionReadRepository _jobPositionReadRepository;
        IJobPositionWriteRepository _jobPositionWriteRepository;
        IJobPositionDeleteRepository _jobPositionDeleteRepository;

        public JobPositionManager(IJobPositionReadRepository jobPositionReadRepository, IJobPositionWriteRepository jobPositionWriteRepository, IJobPositionDeleteRepository jobPositionDeleteRepository)
        {
            _jobPositionReadRepository = jobPositionReadRepository;
            _jobPositionWriteRepository = jobPositionWriteRepository;
            _jobPositionDeleteRepository = jobPositionDeleteRepository;
        }


        [ValidationAspect(typeof(JobPositionValidator))]
        public IResult Add(JobPosition jobPosition)
        {
            _jobPositionWriteRepository.Add(jobPosition);
            return new SuccessResult(Messages.JobPositionAdded);
        }

        [ValidationAspect(typeof(DeleteValidator))]
        public IResult Delete(string id)
        {
            _jobPositionDeleteRepository.Delete(id);
            return new SuccessResult(Messages.JobPositionDeleted);
        }

        public IDataResult<IQueryable<JobPosition>> GetAll()
        {
            return new SuccessDataResult<IQueryable<JobPosition>>(_jobPositionReadRepository.GetAll());
        }

        public IDataResult<JobPosition> GetById(string id)
        {
            return new SuccessDataResult<JobPosition>(_jobPositionReadRepository.GetById(id));
        }

        public IResult JobPositionExists(string jobPosition)
        {
            if (_jobPositionReadRepository.Get(jp => jp.PositionName == jobPosition) != null)
            {
                return new ErrorResult(Messages.JobPositionExists);
            }
            return new SuccessResult();
        }

        public IResult Update(JobPosition jobPosition)
        {
            _jobPositionWriteRepository.Update(jobPosition);
            return new SuccessResult(Messages.JobPositionUpdated);
        }
    }
}