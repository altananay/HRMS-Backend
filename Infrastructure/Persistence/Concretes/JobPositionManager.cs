using Application.Abstractions;
using Application.Aspects;
using Application.CrossCuttingConcerns.Validation.Validators.Common;
using Application.CrossCuttingConcerns.Validation.Validators.JobPositions;
using Application.Repositories;
using Application.Results;
using Application.Utilities.Constants;
using Domain.Entities;
using Persistence.Rules;

namespace Persistence.Concretes
{
    public class JobPositionManager : IJobPositionService
    {
        IJobPositionReadRepository _jobPositionReadRepository;
        IJobPositionWriteRepository _jobPositionWriteRepository;
        IJobPositionDeleteRepository _jobPositionDeleteRepository;
        JobPositionBusinessRules _jobPositionBusinessRules;

        public JobPositionManager(IJobPositionReadRepository jobPositionReadRepository, IJobPositionWriteRepository jobPositionWriteRepository, IJobPositionDeleteRepository jobPositionDeleteRepository, JobPositionBusinessRules jobPositionBusinessRules)
        {
            _jobPositionReadRepository = jobPositionReadRepository;
            _jobPositionWriteRepository = jobPositionWriteRepository;
            _jobPositionDeleteRepository = jobPositionDeleteRepository;
            _jobPositionBusinessRules = jobPositionBusinessRules;
        }


        [ValidationAspect(typeof(JobPositionValidator))]
        //[SecuredOperation("employer")]
        [LogAspect()]
        public async Task<IResult> Add(JobPosition jobPosition)
        {
            _jobPositionBusinessRules.CheckIfJobPositionExistsByName(jobPosition.PositionName);
            await _jobPositionWriteRepository.AddAsync(jobPosition);
            return new SuccessResult(Messages.JobPosition.JobPositionAdded);
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public async Task<IResult> Delete(string id)
        {
            _jobPositionBusinessRules.CheckIfJobPositionExists(id);
            await _jobPositionDeleteRepository.Delete(id);
            return new SuccessResult(Messages.JobPosition.JobPositionDeleted);
        }

        public IDataResult<IQueryable<JobPosition>> GetAll()
        {
            return new SuccessDataResult<IQueryable<JobPosition>>(_jobPositionReadRepository.GetAll());
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<JobPosition> GetById(string id)
        {
            _jobPositionBusinessRules.CheckIfJobPositionExists(id);
            return new SuccessDataResult<JobPosition>(_jobPositionReadRepository.GetById(id));
        }

        [ValidationAspect(typeof(JobPositionValidator))]
        public async Task<IResult> Update(JobPosition jobPosition)
        {
            _jobPositionBusinessRules.CheckIfJobPositionExists(jobPosition.Id);
            await _jobPositionWriteRepository.UpdateAsync(jobPosition);
            return new SuccessResult(Messages.JobPosition.JobPositionUpdated);
        }
    }
}