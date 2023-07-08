using Application.Abstractions;
using Application.Aspects;
using Application.CrossCuttingConcerns.Validation.Validators.Common;
using Application.CrossCuttingConcerns.Validation.Validators.JobSeekers;
using Application.Features.JobSeekers.Commands;
using Application.Repositories;
using Application.Results;
using Application.Utilities.Constants;
using Application.Utilities.Dtos;
using Domain.Entities;
using Persistence.Rules;

namespace Persistence.Concretes
{
    public class JobSeekerManager : IJobSeekerService
    {
        private readonly IJobSeekerWriteRepository _jobSeekerWriteRepository;
        private readonly IJobSeekerDeleteRepository _jobSeekerDeleteRepository;
        private readonly IJobSeekerReadRepository _jobSeekerReadRepository;
        private readonly ICheckPersonService _checkPersonService;
        private readonly IUserService _userService;
        private readonly JobSeekerBusinessRules _rules;

        public JobSeekerManager(IJobSeekerWriteRepository jobSeekerWriteRepository, IJobSeekerDeleteRepository jobSeekerDeleteRepository, IJobSeekerReadRepository jobSeekerReadRepository, ICheckPersonService checkRealPersonService, IUserService userService, JobSeekerBusinessRules rules)
        {
            _jobSeekerWriteRepository = jobSeekerWriteRepository;
            _jobSeekerDeleteRepository = jobSeekerDeleteRepository;
            _jobSeekerReadRepository = jobSeekerReadRepository;
            _checkPersonService = checkRealPersonService;
            _userService = userService;
            _rules = rules;
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
                return new SuccessResult(Messages.User.UserRegistered);
            }
            return new ErrorResult(Messages.Mernis.CitizenError);
        }

        

        [ValidationAspect(typeof(ObjectIdValidator))]
        public async Task<IResult> Delete(string id)
        {
            _rules.CheckIfJobSeekerExists(id);
            try
            {
                await _userService.Delete(id);
                await _jobSeekerDeleteRepository.Delete(id);
                return new SuccessResult(Messages.User.UserDeleted);
            }
            catch (Exception)
            {
                return new ErrorResult(Messages.Common.UnknownError);
            }
        }

        //[SecuredOperation("admin")]
        public IDataResult<IQueryable<JobSeeker>> GetAll()
        {
            return new SuccessDataResult<IQueryable<JobSeeker>>(_jobSeekerReadRepository.GetAll());
        }


        public IDataResult<JobSeeker> GetByMail(string email)
        {
            _rules.CheckIfJobSeekerExistsByEmail(email);
            return new SuccessDataResult<JobSeeker>(_jobSeekerReadRepository.Get(u => u.Email == email));
        }

        [ValidationAspect(typeof(UpdateJobSeekerValidator))]
        public async Task<IResult> Update(UpdateJobSeekerCommand jobSeeker)
        {
            _rules.CheckIfJobSeekerExists(jobSeeker.Id);
            var getJobSeeker = _jobSeekerReadRepository.Get(ni => ni.Id == jobSeeker.Id);
            try
            {
                getJobSeeker.UpdatedAt = DateTime.UtcNow;
                getJobSeeker.Email = jobSeeker.Email;

                await _jobSeekerWriteRepository.UpdateAsync(getJobSeeker);
                return new SuccessResult(Messages.User.UserUpdated);
            }
            catch
            {
                return new ErrorResult(Messages.Common.UnknownError);
            }
        }

        [ValidationAspect(typeof(UpdateJobSeekerValidator))]
        public async Task<IResult> UpdateCvById(string id, Cv cv)
        {
            _rules.CheckIfJobSeekerExists(id);
            var jobSeeker = _jobSeekerReadRepository.Get(ni => ni.Id == id);
            jobSeeker.UpdatedAt = DateTime.UtcNow;
            jobSeeker.Cv = cv;
            await _jobSeekerWriteRepository.UpdateAsync(jobSeeker);
            return new SuccessResult(Messages.Cv.CvUpdated);
        }

        public IDataResult<IQueryable<GetAllJobSeekerDto>> GetAllJobSeeker()
        {
            return new SuccessDataResult<IQueryable<GetAllJobSeekerDto>>(_jobSeekerReadRepository.GetAllJobSeeker());
        }

        public IDataResult<JobSeeker> GetById(string id)
        {
            _rules.CheckIfJobSeekerExists(id);
            return new SuccessDataResult<JobSeeker>(_jobSeekerReadRepository.GetById(id));
        }
    }
}