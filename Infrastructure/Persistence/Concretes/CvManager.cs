using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.CrossCuttingConcerns.Validation.Validators.Common;
using Application.CrossCuttingConcerns.Validation.Validators.Cvs;
using Application.Features.Cvs.Commands;
using Application.Repositories;
using Application.Results;
using Domain.Entities;
using Domain.Objects;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using System.Text;

namespace Persistence.Concretes
{
    public class CvManager : ICVService
    {
        private readonly ICvWriteRepository _cvWriteRepository;
        private readonly ICvDeleteRepository _cvDeleteRepository;
        private readonly ICvReadRepository _cvReadRepository;
        private readonly IJobSeekerReadRepository _jobSeekerReadRepository;
        private readonly IJobSeekerWriteRepository _jobSeekerWriteRepository;
        private readonly ILogger<CvManager> _logger;

        public CvManager(ICvWriteRepository cvWriteRepository, ICvDeleteRepository cvDeleteRepository, ICvReadRepository cvReadRepository, IJobSeekerReadRepository jobSeekerReadRepository, IJobSeekerWriteRepository jobSeekerWriteRepository, ILogger<CvManager> logger)
        {
            _cvWriteRepository = cvWriteRepository;
            _cvDeleteRepository = cvDeleteRepository;
            _cvReadRepository = cvReadRepository;
            _jobSeekerReadRepository = jobSeekerReadRepository;
            _jobSeekerWriteRepository = jobSeekerWriteRepository;
            _logger = logger;
        }

        [ValidationAspect(typeof(CvValidator))]
        [LogAspect("Cv ekleme fonksiyonu başlangıç", true)]
        public async Task<IResult> Add(CreateCvCommand requestCv)
        {
            Cv cv = new();
            JobSeeker oldjobSeeker = _jobSeekerReadRepository.GetById(requestCv.JobSeekerId);
            Project[] projects = new Project[requestCv.Projects.Length];
            JobExperience[] jobExperiences = new JobExperience[requestCv.JobExperiences.Length];

            StringBuilder builder = new();
            builder.Append(requestCv.JobSeekerId);
            
            builder.Append(requestCv.SocialMedias.Github);
            builder.Append(requestCv.SocialMedias.Linkedin);
            builder.Append(requestCv.SocialMedias.WebSite);
            


            for (int i = 0; i < requestCv.Projects.Length; i++)
            {
                Project project = new();
                project.Description = requestCv.Projects[i].Description;
                project.ProjectName = requestCv.Projects[i].ProjectName;

                projects[i] = project;
                builder.Append(project.ProjectName + " " + project.Description);

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
                builder.Append(jobExperience.Years + " " + jobExperience.LeaveWorkYear + " " + jobExperience.Department + " " + jobExperience.Position + " " + jobExperience.CompanyName + " " + jobExperience.Description);
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
            cv.FirstName = oldjobSeeker.FirstName;
            cv.LastName = oldjobSeeker.LastName;
            cv.NationalityId = oldjobSeeker.NationalityId;
            cv.DateOfBirth = oldjobSeeker.DateOfBirth;
            cv.Email = oldjobSeeker.Email;
            cv.Information = requestCv.Information;
            cv.Languages = requestCv.Languages;
            cv.CreatedAt = DateTime.UtcNow;
            cv.JobSeekerId = requestCv.JobSeekerId;

            await _cvWriteRepository.AddAsync(cv);

            oldjobSeeker.Cv = cv;

            await _jobSeekerWriteRepository.UpdateAsync(oldjobSeeker);

            _logger.LogInformation(builder.ToString());

            return new SuccessResult(Messages.CvAdded);
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public async Task<IResult> Delete(string id)
        {
            await _cvDeleteRepository.Delete(id);
            return new SuccessResult(Messages.CvDeleted);
        }

        [LogAspect(true)]
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
        public async Task<IResult> Update(UpdateCvCommand requestCv)
        {
            var oldCv = GetByJobSeekerId(requestCv.JobSeekerId);
            var oldJobSeeker = _jobSeekerReadRepository.GetById(requestCv.JobSeekerId);
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
            newCv.FirstName = oldJobSeeker.FirstName;
            newCv.LastName = oldJobSeeker.LastName;
            newCv.NationalityId = oldJobSeeker.NationalityId;
            newCv.DateOfBirth = oldJobSeeker.DateOfBirth;
            newCv.Email = oldJobSeeker.Email;
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

            oldJobSeeker.Cv = newCv;

            await _jobSeekerWriteRepository.UpdateAsync(oldJobSeeker);

            return new SuccessResult(Messages.CvUpdated);
        }
    }
}