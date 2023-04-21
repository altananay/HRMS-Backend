using Application.Abstractions;
using Application.Aspects;
using Application.Aspects.AutofacAspects;
using Application.Constants;
using Application.Dtos;
using Application.Features.JobSeekers.Commands;
using Application.Repositories;
using Application.Results;
using Application.Validators.Common;
using Application.Validators.JobSeekers;
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
        public async Task<IResult> Add(JobSeeker jobSeeker)
        {
            if (_checkPersonService.CheckPerson())
            {
                var user = new User();
                await _userService.Add(user);
                jobSeeker.Id = user.Id;
                await _jobSeekerWriteRepository.AddAsync(jobSeeker);
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

        [ValidationAspect(typeof(ObjectIdValidator))]
        public async Task<IResult> Delete(string id)
        {
            try
            {
                await _userService.Delete(id);
                await _jobSeekerDeleteRepository.Delete(id);
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

        [ValidationAspect(typeof(UpdateJobSeekerValidator))]
        public async Task<IResult> Update(UpdateJobSeekerCommand jobSeeker)
        {
            var getJobSeeker = _jobSeekerReadRepository.Get(ni => ni.Id == jobSeeker.Id);
            try
            {
                getJobSeeker.UpdatedAt = DateTime.UtcNow;
                getJobSeeker.Email = jobSeeker.Email;

                await _jobSeekerWriteRepository.UpdateAsync(getJobSeeker);
                return new SuccessResult(Messages.UserUpdated);
            }
            catch
            {
                return new ErrorResult(Messages.UnknownError);
            }
        }

        public IDataResult<IQueryable<GetAllJobSeekerDto>> GetAllJobSeeker()
        {
            return new SuccessDataResult<IQueryable<GetAllJobSeekerDto>>(_jobSeekerReadRepository.GetAllJobSeeker());
        }
    }
}