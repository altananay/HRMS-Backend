using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.Dtos;
using Application.Repositories;
using Application.Results;
using Application.Validators;
using Domain.Entities;

namespace Persistence.Concretes
{
    public class JobSeekerManager : IJobSeekerService
    {
        private readonly IJobSeekerWriteRepository _jobSeekerWriteRepository;
        private readonly IJobSeekerDeleteRepository _jobSeekerDeleteRepository;
        private readonly IJobSeekerReadRepository _jobSeekerReadRepository;
        private readonly ICheckPersonService _checkPersonService;
        private readonly IUserService _userService;

        public JobSeekerManager(IJobSeekerWriteRepository jobSeekerWriteRepository, IJobSeekerDeleteRepository jobSeekerDeleteRepository, IJobSeekerReadRepository jobSeekerReadRepository, ICheckPersonService checkRealPersonService, IUserService userService)
        {
            _jobSeekerWriteRepository = jobSeekerWriteRepository;
            _jobSeekerDeleteRepository = jobSeekerDeleteRepository;
            _jobSeekerReadRepository = jobSeekerReadRepository;
            _checkPersonService = checkRealPersonService;
            _userService = userService;
        }

        [ValidationAspect(typeof(JobSeekerValidator))]
        public IResult Add(JobSeeker jobSeeker)
        {
            if (_checkPersonService.CheckIfRealPerson(new MernisCheckDto()
            {
               DateOfBirth = jobSeeker.DateOfBirth,
               NationalityId = jobSeeker.NationalityId,
               FirstName = jobSeeker.FirstName,
               LastName = jobSeeker.LastName,
            }))
            {
                var user = new User();
                _userService.Add(user);
                jobSeeker.Id = user.Id;
                _jobSeekerWriteRepository.Add(jobSeeker);
                return new SuccessResult(Messages.UserRegistered);
            }
            return new ErrorResult(Messages.CitizenError);
        }

        public IResult NationalityIdExists(string nationalityId)
        {
            var result = _jobSeekerReadRepository.Get(js => js.NationalityId == nationalityId);
            if (result == null)
            {
                return new SuccessResult();
            }
            return new ErrorResult(Messages.NationalityIdExists);
        }

        [ValidationAspect(typeof(DeleteValidator))]
        public IResult Delete(string id)
        {
            try
            {
                _userService.Delete(id);
                _jobSeekerDeleteRepository.Delete(id);
                return new SuccessResult(Messages.UserDeleted);
            }
            catch (Exception)
            {
                return new ErrorResult(Messages.UnknownError);
            }
        }

        //[SecuredOperation("admin")]
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
                _jobSeekerWriteRepository.Update(user);
                return new SuccessResult(Messages.UserUpdated);
            }
        }
    }
}