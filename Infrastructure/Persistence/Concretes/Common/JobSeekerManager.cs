using Application.Abstractions.Common;
using Application.Aspects;
using Application.Constants;
using Application.Dtos;
using Application.Repositories;
using Application.Results;
using Application.Utilities.Security.Hashing;
using Application.Validators;
using Domain.Entities;

namespace Persistence.Concretes.Common
{
    public class JobSeekerManager : IJobSeekerService
    {
        private readonly IJobSeekerWriteRepository _jobSeekerWriteRepository;
        private readonly IJobSeekerDeleteRepository _jobSeekerDeleteRepository;
        private readonly IJobSeekerReadRepository _jobSeekerReadRepository;

        public JobSeekerManager(IJobSeekerWriteRepository jobSeekerWriteRepository, IJobSeekerDeleteRepository jobSeekerDeleteRepository, IJobSeekerReadRepository jobSeekerReadRepository)
        {
            _jobSeekerWriteRepository = jobSeekerWriteRepository;
            _jobSeekerDeleteRepository = jobSeekerDeleteRepository;
            _jobSeekerReadRepository = jobSeekerReadRepository;
        }

        //[ValidationAspect(typeof(JobSeekerValidator))]
        public IResult Add(JobSeeker user)
        {
            _jobSeekerWriteRepository.Add(user);
            return new SuccessResult(Messages.UserRegistered);
        }

        public IResult Delete(string id)
        {
            try
            {
                _jobSeekerDeleteRepository.Delete(id);
                return new SuccessResult(Messages.UserDeleted);
            }
            catch (Exception)
            {
                return new ErrorResult(Messages.UnknownError);
            }
        }

        public IDataResult<IQueryable<JobSeeker>> GetAll()
        {
            return new SuccessDataResult<IQueryable<JobSeeker>>(_jobSeekerReadRepository.GetAll());
        }


        public IDataResult<JobSeeker> GetByMail(string email)
        {
            return new SuccessDataResult<JobSeeker>(_jobSeekerReadRepository.Get(u => u.Email == email));
        }

        public IResult Update(JobSeeker user)
        {
            if (user.Id == null)
            {
                return new ErrorResult(Messages.UnknownError);
            }
            else
            {
                return new SuccessResult(Messages.UserUpdated);
            }
        }
    }
}