using Application.Abstractions;
using Application.Aspects;
using Application.CrossCuttingConcerns.Validation.Validators.Common;
using Application.CrossCuttingConcerns.Validation.Validators.Cvs;
using Application.Features.Cvs.Commands;
using Application.Repositories;
using Application.Results;
using Application.Utilities.Constants;
using Domain.Objects;
using MongoDB.Bson;
using Persistence.Rules;
using Cv = Domain.Entities.Cv;

namespace Persistence.Concretes
{
    public class CvManager : ICVService
    {
        private readonly ICvWriteRepository _cvWriteRepository;
        private readonly ICvDeleteRepository _cvDeleteRepository;
        private readonly ICvReadRepository _cvReadRepository;
        private readonly IJobSeekerService _jobSeekerService;
        private readonly CvBusinessRules _rules;

        public CvManager(ICvWriteRepository cvWriteRepository, ICvDeleteRepository cvDeleteRepository, ICvReadRepository cvReadRepository, IJobSeekerService jobSeekerService, CvBusinessRules rules)
        {
            _cvWriteRepository = cvWriteRepository;
            _cvDeleteRepository = cvDeleteRepository;
            _cvReadRepository = cvReadRepository;
            _jobSeekerService = jobSeekerService;
            _rules = rules;
        }

        [ValidationAspect(typeof(CvValidator))]
        [LogAspect("Cv ekleme fonksiyonu başlangıç", true)]
        public async Task<IResult> Add(CreateCvCommand requestCv)
        {
            _rules.CheckIfCvExistsByJobSeekerId(requestCv.JobSeekerId);
            Cv cv = new();

            var oldjobSeeker = _jobSeekerService.GetById(requestCv.JobSeekerId);
            Project[] projects = new Project[requestCv.Projects.Length];
            JobExperience[] jobExperiences = new JobExperience[requestCv.JobExperiences.Length];


            for (int i = 0; i < requestCv.Projects.Length; i++)
            {
                Project project = new();
                project.Description = requestCv.Projects[i].Description;
                project.ProjectName = requestCv.Projects[i].ProjectName;

                projects[i] = project;

            }

            for (int i = 0; i < requestCv.JobExperiences.Length; i++)
            {
                JobExperience jobExperience = new();

                jobExperience.Description = requestCv.JobExperiences[i].Description;
                jobExperience.CompanyName = requestCv.JobExperiences[i].CompanyName;
                jobExperience.Department = requestCv.JobExperiences[i].Department;
                jobExperience.Position = requestCv.JobExperiences[i].Position;
                jobExperience.Years = requestCv.JobExperiences[i].Years;
                jobExperience.LeaveWorkYear = requestCv.JobExperiences[i].LeaveWorkYear;

                jobExperiences[i] = jobExperience;
                
            }


            foreach (var project in projects)
            {
                ObjectId id = ObjectId.GenerateNewId();
                project.Id = id.ToString();
                project.CreatedAt = DateTime.UtcNow;
                project.UpdatedAt = null;
            }



            foreach (var jobExperience in jobExperiences)
            {
                ObjectId id = ObjectId.GenerateNewId();
                jobExperience.Id = id.ToString();
                jobExperience.CreatedAt = DateTime.UtcNow;
                jobExperience.UpdatedAt = null;
            }

            cv.Educations = requestCv.Educations;
            cv.SocialMedias = requestCv.SocialMedias;
            cv.Skills = requestCv.Skills;
            cv.Projects = projects;
            cv.Hobbies = requestCv.Hobbies;
            cv.ImageUrl = requestCv.ImageUrl;
            cv.JobExperiences = jobExperiences;
            cv.FirstName = oldjobSeeker.Data.FirstName;
            cv.LastName = oldjobSeeker.Data.LastName;
            cv.NationalityId = oldjobSeeker.Data.NationalityId;
            cv.DateOfBirth = oldjobSeeker.Data.DateOfBirth;
            cv.Email = oldjobSeeker.Data.Email;
            cv.Information = requestCv.Information;
            cv.Languages = requestCv.Languages;
            cv.CreatedAt = DateTime.UtcNow;
            cv.JobSeekerId = requestCv.JobSeekerId;

            await _cvWriteRepository.AddAsync(cv);

            oldjobSeeker.Data.Cv = cv;

            await _jobSeekerService.UpdateCvById(oldjobSeeker.Data.Id, oldjobSeeker.Data.Cv);


            return new SuccessResult(Messages.Cv.CvAdded);
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public async Task<IResult> Delete(string id)
        {
            _rules.CheckIfCvExists(id);
            await _cvDeleteRepository.Delete(id);
            return new SuccessResult(Messages.Cv.CvDeleted);
        }

        [LogAspect(true)]
        public IDataResult<IQueryable<Cv>> GetAll()
        {
            return new SuccessDataResult<IQueryable<Cv>>(_cvReadRepository.GetAll());
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<Cv> GetByJobSeekerId(string id)
        {
            _rules.CheckIfCvExists(id);
            return new SuccessDataResult<Cv>(_cvReadRepository.Get(cv => cv.JobSeekerId == id));
        }

        [ValidationAspect(typeof(UpdateCvValidator))]
        public async Task<IResult> Update(UpdateCvCommand requestCv)
        {
            _rules.CheckIfCvExists(requestCv.Id);
            _rules.CheckIfCvExistsByJobSeekerId(requestCv.JobSeekerId);
            var oldCv = GetByJobSeekerId(requestCv.JobSeekerId);
            var oldJobSeeker = _jobSeekerService.GetById(requestCv.JobSeekerId);
            Project[] projects = new Project[requestCv.Projects.Length];
            JobExperience[] jobExperiences = new JobExperience[requestCv.JobExperiences.Length];

            for (int i = 0; i < requestCv.Projects.Length; i++)
            {
                Project project = new();
                project.Description = requestCv.Projects[i].Description;
                project.ProjectName = requestCv.Projects[i].ProjectName;

                projects[i] = project;

            }

            for (int i = 0; i < requestCv.JobExperiences.Length; i++)
            {
                JobExperience jobExperience = new();

                jobExperience.Description = requestCv.JobExperiences[i].Description;
                jobExperience.CompanyName = requestCv.JobExperiences[i].CompanyName;
                jobExperience.Department = requestCv.JobExperiences[i].Department;
                jobExperience.Position = requestCv.JobExperiences[i].Position;
                jobExperience.Years = requestCv.JobExperiences[i].Years;
                jobExperience.LeaveWorkYear = requestCv.JobExperiences[i].LeaveWorkYear;

                jobExperiences[i] = jobExperience;
            }


            foreach (var project in projects)
            {
                ObjectId id = ObjectId.GenerateNewId();
                project.Id = id.ToString();
                project.CreatedAt = DateTime.UtcNow;
                project.UpdatedAt = null;
            }



            foreach (var jobExperience in jobExperiences)
            {
                ObjectId id = ObjectId.GenerateNewId();
                jobExperience.Id = id.ToString();
                jobExperience.CreatedAt = DateTime.UtcNow;
                jobExperience.UpdatedAt = null;
            }


            Cv newCv = new();
            newCv.Id = requestCv.Id;
            newCv.JobSeekerId = requestCv.JobSeekerId;
            newCv.FirstName = oldJobSeeker.Data.FirstName;
            newCv.LastName = oldJobSeeker.Data.LastName;
            newCv.NationalityId = oldJobSeeker.Data.NationalityId;
            newCv.DateOfBirth = oldJobSeeker.Data.DateOfBirth;
            newCv.Email = oldJobSeeker.Data.Email;
            newCv.CreatedAt = oldCv.Data.CreatedAt;
            newCv.UpdatedAt = DateTime.UtcNow;
            newCv.Projects = projects;
            newCv.Skills = requestCv.Skills;
            newCv.Hobbies = requestCv.Hobbies;
            newCv.Educations = requestCv.Educations;
            newCv.SocialMedias = requestCv.SocialMedias;
            newCv.ImageUrl = requestCv.ImageUrl;
            newCv.Information = requestCv.Information;
            newCv.JobExperiences = jobExperiences;
            newCv.Languages = requestCv.Languages;

            await _cvWriteRepository.UpdateAsync(newCv);

            oldJobSeeker.Data.Cv = newCv;


            await _jobSeekerService.UpdateCvById(oldJobSeeker.Data.Id, oldJobSeeker.Data.Cv);

            return new SuccessResult(Messages.Cv.CvUpdated);
        }

        
    }
}