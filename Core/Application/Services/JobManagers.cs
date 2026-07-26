using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Common.Dtos;
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

        public async Task<IDataResult<PagedResult<JobAdvertisementDto>>> GetPagedAsync(
            PageRequest page,
            Guid? employerId = null,
            bool? isActive = null,
            bool orderByHighestSalary = false,
            CancellationToken cancellationToken = default)
        {
            var result = await _advertisements.GetPagedAsync(
                page, employerId, isActive, orderByHighestSalary, cancellationToken);

            return new SuccessDataResult<PagedResult<JobAdvertisementDto>>(new PagedResult<JobAdvertisementDto>(
                result.Items.Select(DomainMapper.ToDto).ToList(), result.Page, result.PageSize, result.TotalCount));
        }

        public async Task<IDataResult<JobAdvertisementDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var advertisement = await _advertisements.GetByIdWithDetailsAsync(id, cancellationToken)
                ?? throw new NotFoundException(Messages.JobAdvertisement.NotFound);

            return new SuccessDataResult<JobAdvertisementDto>(DomainMapper.ToDto(advertisement));
        }

        public async Task<IResult> AddAsync(CreateJobAdvertisementCommand command, CancellationToken cancellationToken = default)
        {
            // Verify the employer BEFORE writing anything. The old flow inserted a JobPosition
            // first and only then looked the employer up, so a bad employer id left an orphan
            // position behind — with no transaction to roll it back.
            await _rules.EnsureEmployerExistsAsync(command.EmployerId, cancellationToken);

            var position = await _positions.ResolveOrCreateAsync(command.JobPositionName, cancellationToken);

            _advertisements.Add(new JobAdvertisement
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
            });

            // One SaveChanges, so the position and the advertisement land in a single transaction.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.JobAdvertisement.Added);
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

            // Explicitly carried. The old Update never restored Status, so editing an advertisement
            // deactivated it every time.
            advertisement.IsActive = command.IsActive;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.JobAdvertisement.Updated);
        }

        public async Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var advertisement = await _rules.EnsureJobAdvertisementExistsAsync(id, cancellationToken);

            // Soft delete, and the shared JobPosition is left alone — the old Delete removed the
            // position along with the advertisement, which only made sense while positions were
            // created one-per-advertisement.
            _advertisements.Remove(advertisement);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.JobAdvertisement.Deleted);
        }

        /// <summary>
        /// Rejects an employer acting on somebody else's advertisement.
        /// </summary>
        /// <remarks>
        /// There was no such check. EmployerId arrived in the request body, so any caller could edit
        /// or delete any advertisement — role membership alone does not establish ownership.
        /// </remarks>
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

        public JobApplicationManager(
            IJobApplicationRepository applications,
            IUnitOfWork unitOfWork,
            BusinessRules rules,
            TimeProvider timeProvider)
        {
            _applications = applications;
            _unitOfWork = unitOfWork;
            _rules = rules;
            _timeProvider = timeProvider;
        }

        public async Task<IDataResult<PagedResult<JobApplicationDto>>> GetPagedAsync(
            PageRequest page,
            Guid? employerId = null,
            Guid? jobSeekerId = null,
            JobApplicationStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _applications.GetPagedAsync(page, employerId, jobSeekerId, status, cancellationToken);

            return new SuccessDataResult<PagedResult<JobApplicationDto>>(new PagedResult<JobApplicationDto>(
                result.Items.Select(DomainMapper.ToDto).ToList(), result.Page, result.PageSize, result.TotalCount));
        }

        public async Task<IDataResult<JobApplicationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var application = await _applications.GetByIdWithDetailsAsync(id, cancellationToken)
                ?? throw new NotFoundException(Messages.JobApplication.NotFound);

            return new SuccessDataResult<JobApplicationDto>(DomainMapper.ToDto(application));
        }

        public async Task<IResult> AddAsync(CreateJobApplicationCommand command, CancellationToken cancellationToken = default)
        {
            var advertisement = await _rules.EnsureJobAdvertisementExistsAsync(command.JobAdvertisementId, cancellationToken);
            await _rules.EnsureJobSeekerExistsAsync(command.JobSeekerId, cancellationToken);

            if (!advertisement.IsActive)
            {
                throw new ConflictException(Messages.JobAdvertisement.NotFound);
            }

            await _rules.EnsureNotAlreadyAppliedAsync(command.JobSeekerId, command.JobAdvertisementId, cancellationToken);

            _applications.Add(new JobApplication
            {
                JobAdvertisementId = command.JobAdvertisementId,
                JobSeekerId = command.JobSeekerId,
                JobSeekerNote = command.JobSeekerNote,
                Status = JobApplicationStatus.Submitted,
                StatusChangedAt = _timeProvider.GetUtcNow().UtcDateTime
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.JobApplication.Added);
        }

        public async Task<IResult> UpdateAsync(UpdateJobApplicationCommand command, CancellationToken cancellationToken = default)
        {
            var application = await _applications.GetByIdWithDetailsAsync(command.Id, cancellationToken)
                ?? throw new NotFoundException(Messages.JobApplication.NotFound);

            // The advertisement carries the employer, so ownership is checked through the join
            // rather than a denormalized column that could disagree with it.
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

        public CvManager(ICvRepository cvs, IUnitOfWork unitOfWork, BusinessRules rules)
        {
            _cvs = cvs;
            _unitOfWork = unitOfWork;
            _rules = rules;
        }

        public async Task<IDataResult<PagedResult<CvDto>>> GetPagedAsync(
            PageRequest page,
            CancellationToken cancellationToken = default)
        {
            var result = await _cvs.GetPagedAsync(page, cancellationToken);

            return new SuccessDataResult<PagedResult<CvDto>>(new PagedResult<CvDto>(
                result.Items.Select(DomainMapper.ToDto).ToList(), result.Page, result.PageSize, result.TotalCount));
        }

        public async Task<IDataResult<CvDto>> GetByJobSeekerIdAsync(Guid jobSeekerId, CancellationToken cancellationToken = default)
        {
            // Looks the CV up BY seeker id. The old GetByJobSeekerId passed the seeker id into
            // CheckIfCvExists, which validates CV ids — so it failed for every caller whose CV id
            // did not happen to equal their own.
            var cv = await _rules.EnsureCvExistsForJobSeekerAsync(jobSeekerId, cancellationToken);

            return new SuccessDataResult<CvDto>(DomainMapper.ToDto(cv));
        }

        public async Task<IResult> AddAsync(CreateCvCommand command, CancellationToken cancellationToken = default)
        {
            await _rules.EnsureJobSeekerExistsAsync(command.JobSeekerId, cancellationToken);
            await _rules.EnsureCvDoesNotExistForJobSeekerAsync(command.JobSeekerId, cancellationToken);

            var cv = new Cv { JobSeekerId = command.JobSeekerId };
            ApplyTo(cv, command.Information, command.ImageUrl, command.Hobbies, command.Skills,
                command.SocialMedia, command.Educations, command.JobExperiences, command.Languages, command.Projects);

            _cvs.Add(cv);

            // A single write. The old Add inserted the CV and then called
            // JobSeekerManager.UpdateCvById to embed a second copy inside the seeker document —
            // two writes, no transaction, and two copies free to drift apart.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.Cv.Added);
        }

        public async Task<IResult> UpdateAsync(UpdateCvCommand command, CancellationToken cancellationToken = default)
        {
            // Requires the CV to EXIST. The old Update called CheckIfCvExistsByJobSeekerId, which
            // throws when one is found, so this operation failed unconditionally.
            var cv = await _rules.EnsureCvExistsForJobSeekerAsync(command.JobSeekerId, cancellationToken);

            ApplyTo(cv, command.Information, command.ImageUrl, command.Hobbies, command.Skills,
                command.SocialMedia, command.Educations, command.JobExperiences, command.Languages, command.Projects);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.Cv.Updated);
        }

        public async Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cv = await _cvs.GetByIdWithDetailsAsync(id, cancellationToken)
                ?? throw new NotFoundException(Messages.Cv.NotFound);

            _cvs.Remove(cv);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.Cv.Deleted);
        }

        /// <summary>
        /// Applies the scalar fields and replaces the child collections.
        /// </summary>
        /// <remarks>
        /// Shared by Add and Update — those two methods previously carried the same
        /// project/experience construction loops duplicated verbatim.
        /// </remarks>
        private static void ApplyTo(
            Cv cv,
            string? information,
            string? imageUrl,
            string? hobbies,
            string[] skills,
            SocialMediaInput? socialMedia,
            List<EducationInput> educations,
            List<JobExperienceInput> jobExperiences,
            List<CvLanguageInput> languages,
            List<CvProjectInput> projects)
        {
            cv.Information = information;
            cv.ImageUrl = imageUrl;
            cv.Hobbies = hobbies;
            cv.Skills = skills;

            // Always assigned: the owned instance is a required navigation now, so null would fail on
            // save. An omitted block still means "clear the links" — the three columns end up null
            // either way, so this stores exactly what the previous conditional stored.
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
