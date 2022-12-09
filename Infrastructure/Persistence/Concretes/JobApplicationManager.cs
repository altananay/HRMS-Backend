using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.Repositories;
using Application.Results;
using Application.Validators;
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

        public IResult Add(JobApplication jobApplication)
        {
            _jobApplicationWriteRepository.AddAsync(jobApplication);
            return new SuccessResult(Messages.JobApplicationMade);
        }


        [ValidationAspect(typeof(DeleteValidator))]
        public IResult Delete(string id)
        {
            _jobApplicationDeleteRepository.Delete(id);
            return new SuccessResult(Messages.JobApplicationDeleted);
        }

        public IDataResult<IQueryable<JobApplication>> GetAll()
        {
            return new SuccessDataResult<IQueryable<JobApplication>>(_jobApplicationReadRepository.GetAll());
        }

        public IDataResult<JobApplication> GetById(string id)
        {
            return new SuccessDataResult<JobApplication>(_jobApplicationReadRepository.GetById(id));
        }

        public IResult Update(JobApplication jobApplication)
        {
            _jobApplicationWriteRepository.UpdateAsync(jobApplication);
            return new SuccessResult(Messages.JobApplicationUpdated);
        }
    }
}