using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Common.Contracts;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Features.Cvs.Commands;
using Application.Features.JobAdvertisements.Commands;
using Application.Features.JobApplications.Commands;
using Application.Mapping;
using Application.Results;
using Application.Rules;
using Application.Utilities.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Services
{
    public sealed class JobAdvertisementManager : IJobAdvertisementService
    {
        private readonly IJobAdvertisementRepository _advertisements;
        private readonly IJobPositionRepository _positions;
        private readonly IUnitOfWork _unitOfWork;
        private readonly BusinessRules _rules;

        public JobAdvertisementManager(
            IJobAdvertisementRepository advertisements,
            IJobPositionRepository positions,
            IUnitOfWork unitOfWork,
            BusinessRules rules)
        {
            _advertisements = advertisements;
            _positions = positions;
            _unitOfWork = unitOfWork;
            _rules = rules;
        }

        public async Task<IDataResult<PagedResult<JobAdvertisementResponse>>> GetPagedAsync(
            PageRequest page,
            Guid? employerId = null,
            bool? isActive = null,
            bool orderByHighestSalary = false,
            CancellationToken cancellationToken = default)
        {
            var result = await _advertisements.GetPagedAsync(
                page, employerId, isActive, orderByHighestSalary, cancellationToken);

            return new SuccessDataResult<PagedResult<JobAdvertisementResponse>>(new PagedResult<JobAdvertisementResponse>(
                result.Items.Select(DomainMapper.ToResponse).ToList(), result.Page, result.PageSize, result.TotalCount));
        }

        public async Task<IDataResult<JobAdvertisementResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var advertisement = await _advertisements.GetByIdWithDetailsAsync(id, cancellationToken)
                ?? throw new NotFoundException(Messages.JobAdvertisement.NotFound);

            return new SuccessDataResult<JobAdvertisementResponse>(DomainMapper.ToResponse(advertisement));
        }

        public async Task<IDataResult<CreatedResponse>> AddAsync(CreateJobAdvertisementCommand command, CancellationToken cancellationToken = default)
        {
            await _rules.EnsureEmployerExistsAsync(command.EmployerId, cancellationToken);

            var position = await _positions.ResolveOrCreateAsync(command.JobPositionName, cancellationToken);

            var advertisement = new JobAdvertisement
            {
                EmployerId = command.EmployerId,
                JobPosition = position,
                Title = command.Title,
                Description = command.Description,
                Experience = command.Experience,
                Skills = command.Skills,
                City = command.City,
                MinSalary = command.MinSalary,
                MaxSalary = command.MaxSalary,
                Currency = command.Currency,
                OpenPositions = command.OpenPositions,
                JobType = command.JobType,
                Deadline = command.Deadline,
                IsActive = true
            };

            _advertisements.Add(advertisement);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessDataResult<CreatedResponse>(
                new CreatedResponse(advertisement.Id), Messages.JobAdvertisement.Added);
        }

        public async Task<IResult> UpdateAsync(UpdateJobAdvertisementCommand command, CancellationToken cancellationToken = default)
        {
            var advertisement = await _rules.EnsureJobAdvertisementExistsAsync(command.Id, cancellationToken);

            EnsureOwnedBy(advertisement, command.EmployerId);

            var position = await _positions.ResolveOrCreateAsync(command.JobPositionName, cancellationToken);

            advertisement.JobPosition = position;
            advertisement.Title = command.Title;
            advertisement.Description = command.Description;
            advertisement.Experience = command.Experience;
            advertisement.Skills = command.Skills;
            advertisement.City = command.City;
            advertisement.MinSalary = command.MinSalary;
            advertisement.MaxSalary = command.MaxSalary;
            advertisement.Currency = command.Currency;
            advertisement.OpenPositions = command.OpenPositions;
            advertisement.JobType = command.JobType;
            advertisement.Deadline = command.Deadline;

            advertisement.IsActive = command.IsActive;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.JobAdvertisement.Updated);
        }

        public async Task<IResult> DeleteAsync(Guid id, Guid employerId, CancellationToken cancellationToken = default)
        {
            var advertisement = await _rules.EnsureJobAdvertisementExistsAsync(id, cancellationToken);

            EnsureOwnedBy(advertisement, employerId);

            _advertisements.Remove(advertisement);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.JobAdvertisement.Deleted);
        }

        private static void EnsureOwnedBy(JobAdvertisement advertisement, Guid employerId)
        {
            if (advertisement.EmployerId != employerId)
            {
                throw new ForbiddenException(Messages.Authentication.AuthorizationDenied);
            }
        }
    }

    public sealed class JobApplicationManager : IJobApplicationService
    {
        private readonly IJobApplicationRepository _applications;
        private readonly IUnitOfWork _unitOfWork;
        private readonly BusinessRules _rules;
        private readonly TimeProvider _timeProvider;
        private readonly ICurrentUserService _currentUser;

        public JobApplicationManager(
            IJobApplicationRepository applications,
            IUnitOfWork unitOfWork,
            BusinessRules rules,
            TimeProvider timeProvider,
            ICurrentUserService currentUser)
        {
            _applications = applications;
            _unitOfWork = unitOfWork;
            _rules = rules;
            _timeProvider = timeProvider;
            _currentUser = currentUser;
        }

        public async Task<IDataResult<PagedResult<JobApplicationResponse>>> GetPagedAsync(
            PageRequest page,
            Guid? employerId = null,
            Guid? jobSeekerId = null,
            JobApplicationStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _applications.GetPagedAsync(page, employerId, jobSeekerId, status, cancellationToken);

            return new SuccessDataResult<PagedResult<JobApplicationResponse>>(new PagedResult<JobApplicationResponse>(
                result.Items.Select(DomainMapper.ToResponse).ToList(), result.Page, result.PageSize, result.TotalCount));
        }

        public async Task<IDataResult<JobApplicationResponse>> GetByIdAsync(Guid id, Guid requestedBy, CancellationToken cancellationToken = default)
        {
            var application = await _applications.GetByIdWithDetailsAsync(id, cancellationToken)
                ?? throw new NotFoundException(Messages.JobApplication.NotFound);

            var isApplicant = application.JobSeekerId == requestedBy;
            var isOwningEmployer = application.JobAdvertisement.EmployerId == requestedBy;

            if (!isApplicant && !isOwningEmployer && !_currentUser.IsInRole(Roles.Admin))
            {
                throw new ForbiddenException(Messages.Authentication.AuthorizationDenied);
            }

            return new SuccessDataResult<JobApplicationResponse>(DomainMapper.ToResponse(application));
        }

        public async Task<IDataResult<CreatedResponse>> AddAsync(CreateJobApplicationCommand command, CancellationToken cancellationToken = default)
        {
            var advertisement = await _rules.EnsureJobAdvertisementExistsAsync(command.JobAdvertisementId, cancellationToken);
            await _rules.EnsureJobSeekerExistsAsync(command.JobSeekerId, cancellationToken);

            if (!advertisement.IsActive)
            {
                throw new ConflictException(Messages.JobAdvertisement.NotFound);
            }

            await _rules.EnsureNotAlreadyAppliedAsync(command.JobSeekerId, command.JobAdvertisementId, cancellationToken);

            var application = new JobApplication
            {
                JobAdvertisementId = command.JobAdvertisementId,
                JobSeekerId = command.JobSeekerId,
                JobSeekerNote = command.JobSeekerNote,
                Status = JobApplicationStatus.Submitted,
                StatusChangedAt = _timeProvider.GetUtcNow().UtcDateTime
            };

            _applications.Add(application);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessDataResult<CreatedResponse>(
                new CreatedResponse(application.Id), Messages.JobApplication.Added);
        }

        public async Task<IResult> UpdateAsync(UpdateJobApplicationCommand command, CancellationToken cancellationToken = default)
        {
            var application = await _applications.GetByIdWithDetailsAsync(command.Id, cancellationToken)
                ?? throw new NotFoundException(Messages.JobApplication.NotFound);

            if (application.JobAdvertisement.EmployerId != command.EmployerId)
            {
                throw new ForbiddenException(Messages.Authentication.AuthorizationDenied);
            }

            if (application.Status != command.Status)
            {
                application.Status = command.Status;
                application.StatusChangedAt = _timeProvider.GetUtcNow().UtcDateTime;
            }

            application.EmployerNote = command.EmployerNote;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.JobApplication.Updated);
        }

        public async Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var application = await _rules.EnsureJobApplicationExistsAsync(id, cancellationToken);

            _applications.Remove(application);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.JobApplication.Deleted);
        }
    }

    public sealed class CvManager : ICvService
    {
        private readonly ICvRepository _cvs;
        private readonly IUnitOfWork _unitOfWork;
        private readonly BusinessRules _rules;
        private readonly CandidateAccessPolicy _access;

        public CvManager(
            ICvRepository cvs,
            IUnitOfWork unitOfWork,
            BusinessRules rules,
            CandidateAccessPolicy access)
        {
            _cvs = cvs;
            _unitOfWork = unitOfWork;
            _rules = rules;
            _access = access;
        }

        public async Task<IDataResult<PagedResult<CvResponse>>> GetPagedAsync(
            PageRequest page,
            CancellationToken cancellationToken = default)
        {
            var result = await _cvs.GetPagedAsync(page, cancellationToken);

            return new SuccessDataResult<PagedResult<CvResponse>>(new PagedResult<CvResponse>(
                result.Items.Select(DomainMapper.ToResponse).ToList(), result.Page, result.PageSize, result.TotalCount));
        }

        public async Task<IDataResult<CvResponse>> GetByJobSeekerIdAsync(Guid jobSeekerId, Guid requestedBy, CancellationToken cancellationToken = default)
        {
            await _access.EnsureCanReadAsync(jobSeekerId, requestedBy, cancellationToken);

            var cv = await _rules.EnsureCvExistsForJobSeekerAsync(jobSeekerId, cancellationToken);

            return new SuccessDataResult<CvResponse>(DomainMapper.ToResponse(cv));
        }

        public async Task<IDataResult<CreatedResponse>> AddAsync(CreateCvCommand command, CancellationToken cancellationToken = default)
        {
            await _rules.EnsureJobSeekerExistsAsync(command.JobSeekerId, cancellationToken);
            await _rules.EnsureCvDoesNotExistForJobSeekerAsync(command.JobSeekerId, cancellationToken);

            var cv = new Cv { JobSeekerId = command.JobSeekerId };
            ApplyTo(cv, command.Information, command.ImageUrl, command.Hobbies, command.Skills,
                command.SocialMedia, command.Educations, command.JobExperiences, command.Languages, command.Projects);

            _cvs.Add(cv);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessDataResult<CreatedResponse>(new CreatedResponse(cv.Id), Messages.Cv.Added);
        }

        public async Task<IResult> UpdateAsync(UpdateCvCommand command, CancellationToken cancellationToken = default)
        {
            var cv = await _rules.EnsureCvExistsForJobSeekerAsync(command.JobSeekerId, cancellationToken);

            ApplyTo(cv, command.Information, command.ImageUrl, command.Hobbies, command.Skills,
                command.SocialMedia, command.Educations, command.JobExperiences, command.Languages, command.Projects);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.Cv.Updated);
        }

        public async Task<IResult> DeleteAsync(Guid id, Guid requestedBy, CancellationToken cancellationToken = default)
        {
            var cv = await _cvs.GetByIdWithDetailsAsync(id, cancellationToken)
                ?? throw new NotFoundException(Messages.Cv.NotFound);

            // cvs has no deleted_at: this is permanent and cascades to every child row.
            _access.EnsureCanModify(cv.JobSeekerId, requestedBy);

            _cvs.Remove(cv);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.Cv.Deleted);
        }

        private static void ApplyTo(
            Cv cv,
            string? information,
            string? imageUrl,
            string? hobbies,
            string[] skills,
            SocialMediaRequest? socialMedia,
            List<EducationRequest> educations,
            List<JobExperienceRequest> jobExperiences,
            List<CvLanguageRequest> languages,
            List<CvProjectRequest> projects)
        {
            cv.Information = information;
            cv.ImageUrl = imageUrl;
            cv.Hobbies = hobbies;
            cv.Skills = skills;

            cv.SocialMedia = new SocialMedia
            {
                Github = socialMedia?.Github,
                Linkedin = socialMedia?.Linkedin,
                WebSite = socialMedia?.WebSite
            };

            cv.Educations.Clear();
            foreach (var education in educations)
            {
                cv.Educations.Add(new Education
                {
                    School = education.School,
                    Major = education.Major,
                    Grade = education.Grade,
                    StartYear = education.StartYear,
                    EndYear = education.EndYear,
                    IsGraduated = education.IsGraduated
                });
            }

            cv.JobExperiences.Clear();
            foreach (var experience in jobExperiences)
            {
                cv.JobExperiences.Add(new JobExperience
                {
                    CompanyName = experience.CompanyName,
                    Department = experience.Department,
                    Position = experience.Position,
                    StartYear = experience.StartYear,
                    EndYear = experience.EndYear,
                    Description = experience.Description
                });
            }

            cv.Languages.Clear();
            foreach (var language in languages)
            {
                cv.Languages.Add(new CvLanguage { Name = language.Name, Level = language.Level });
            }

            cv.Projects.Clear();
            foreach (var project in projects)
            {
                cv.Projects.Add(new CvProject { Name = project.Name, Description = project.Description });
            }
        }
    }
}
