using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.Features.Cvs.Commands;
using Application.Repositories;
using Application.Results;
using Application.Validators.Common;
using Application.Validators.Cvs;
using Domain.Entities;

namespace Persistence.Concretes
{
    public class CvManager : ICVService
    {
        private readonly ICvWriteRepository _cvWriteRepository;
        private readonly ICvDeleteRepository _cvDeleteRepository;
        private readonly ICvReadRepository _cvReadRepository;
        private readonly IJobSeekerReadRepository _jobSeekerReadRepository;
        private readonly IJobSeekerWriteRepository _jobSeekerWriteRepository;

        public CvManager(ICvWriteRepository cvWriteRepository, ICvDeleteRepository cvDeleteRepository, ICvReadRepository cvReadRepository, IJobSeekerReadRepository jobSeekerReadRepository, IJobSeekerWriteRepository jobSeekerWriteRepository)
        {
            _cvWriteRepository = cvWriteRepository;
            _cvDeleteRepository = cvDeleteRepository;
            _cvReadRepository = cvReadRepository;
            _jobSeekerReadRepository = jobSeekerReadRepository;
            _jobSeekerWriteRepository = jobSeekerWriteRepository;
        }

        [ValidationAspect(typeof(CvValidator))]
        public async Task<IResult> Add(CreateCvCommand requestCv)
        {
            

            Cv cv = new();
            cv.Educations = requestCv.Educations;
            cv.SocialMedias = requestCv.SocialMedias;
            cv.Skills = requestCv.Skills;
            cv.Projects = requestCv.Projects;
            cv.Educations = requestCv.Educations;
            cv.Hobbies = requestCv.Hobbies;
            cv.ImageUrl = requestCv.ImageUrl;
            cv.ProgrammingLanguages = requestCv.ProgrammingLanguages;
            cv.JobExperiences = requestCv.JobExperiences;
            cv.JobSeekerId = requestCv.JobSeekerId;
            cv.Information = requestCv.Information;
            cv.Languages = requestCv.Languages;
            cv.CreatedAt = DateTime.UtcNow;
            await _cvWriteRepository.AddAsync(cv);

            var result = _jobSeekerReadRepository.GetById(requestCv.JobSeekerId);
            result.CvId = cv.Id;
            await _jobSeekerWriteRepository.UpdateAsync(result);

            return new SuccessResult(Messages.CvAdded);
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public async Task<IResult> Delete(string id)
        {
            await _cvDeleteRepository.Delete(id);
            return new SuccessResult(Messages.CvDeleted);
        }

        public IDataResult<IQueryable<Cv>> GetAll()
        {
            return new SuccessDataResult<IQueryable<Cv>>(_cvReadRepository.GetAll());
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<Cv> GetByJobSeekerId(string id)
        {
            return new SuccessDataResult<Cv>(_cvReadRepository.Get(cv => cv.JobSeekerId == id));
        }


        [ValidationAspect(typeof(UpdateCvValidator))]
        public async Task<IResult> Update(UpdateCvCommand cv)
        {
            var oldCv = GetByJobSeekerId(cv.JobSeekerId);

            Cv newCv = new();
            newCv.Id = cv.Id;
            newCv.CreatedAt = oldCv.Data.CreatedAt;
            newCv.UpdatedAt = DateTime.UtcNow;
            newCv.Projects = cv.Projects;
            newCv.Skills = cv.Skills;
            newCv.Hobbies = cv.Hobbies;
            newCv.Educations = cv.Educations;
            newCv.SocialMedias = cv.SocialMedias;
            newCv.ImageUrl = cv.ImageUrl;
            newCv.Information = cv.Information;
            newCv.JobExperiences = cv.JobExperiences;
            newCv.Languages = cv.Languages;
            newCv.ProgrammingLanguages = cv.ProgrammingLanguages;
            newCv.JobSeekerId = cv.JobSeekerId;

            await _cvWriteRepository.UpdateAsync(newCv);
            return new SuccessResult(Messages.CvUpdated);
        }
    }
}