using Application.Features.Contacts.Commands;
using Application.Features.Cvs.Commands;
using Application.Features.Employers.Commands;
using Application.Features.JobAdvertisements.Commands;
using Application.Features.JobApplications.Commands;
using Application.Features.JobPositions.Commands;
using Application.Features.JobSeekers.Commands;
using Application.Features.SystemStaffs.Commands;
using FluentValidation;

namespace Application.Validation
{
    /// <summary>
    /// Request validators, discovered by <c>AddValidatorsFromAssembly</c> and executed by
    /// ValidationBehavior.
    /// </summary>
    /// <remarks>
    /// These validate the MediatR <b>request</b>, not the entity. The old set targeted entities
    /// (<c>AbstractValidator&lt;JobSeeker&gt;</c>) and was invoked by a Castle attribute that
    /// matched constructor arguments by runtime type, which is why the 21 ObjectId validators never
    /// fired: their target was <c>string</c>, and the aspect's type comparison silently skipped it.
    ///
    /// The ObjectId validators are gone entirely — ids are Guids and the <c>{id:guid}</c> route
    /// constraint rejects malformed values during routing, before any handler runs. (An anonymous
    /// caller sees 401 rather than 404 for a malformed id, because an unmatched request carries no
    /// endpoint metadata for <c>[AllowAnonymous]</c> to apply to and the authorization fallback
    /// policy challenges it first.)
    /// </remarks>
    public sealed class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
    {
        public CreateContactCommandValidator()
        {
            RuleFor(command => command.FirstName).NotEmpty().MinimumLength(2).MaximumLength(100);
            RuleFor(command => command.LastName).NotEmpty().MinimumLength(2).MaximumLength(100);
            RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(command => command.Subject).NotEmpty().MinimumLength(5).MaximumLength(200);
            RuleFor(command => command.Message).NotEmpty().MinimumLength(20).MaximumLength(4000);
        }
    }

    public sealed class UpdateContactCommandValidator : AbstractValidator<UpdateContactCommand>
    {
        public UpdateContactCommandValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(command => command.LastName).NotEmpty().MaximumLength(100);
            RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(command => command.Subject).NotEmpty().MaximumLength(200);
            RuleFor(command => command.Message).NotEmpty().MaximumLength(4000);
        }
    }

    public sealed class CreateJobPositionCommandValidator : AbstractValidator<CreateJobPositionCommand>
    {
        public CreateJobPositionCommandValidator()
            => RuleFor(command => command.Name).NotEmpty().MinimumLength(2).MaximumLength(200);
    }

    public sealed class UpdateJobPositionCommandValidator : AbstractValidator<UpdateJobPositionCommand>
    {
        public UpdateJobPositionCommandValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.Name).NotEmpty().MinimumLength(2).MaximumLength(200);
        }
    }

    public sealed class CreateJobAdvertisementCommandValidator : AbstractValidator<CreateJobAdvertisementCommand>
    {
        public CreateJobAdvertisementCommandValidator()
        {
            RuleFor(command => command.EmployerId).NotEmpty();
            RuleFor(command => command.Title).NotEmpty().MinimumLength(3).MaximumLength(200);
            RuleFor(command => command.JobPositionName).NotEmpty().MaximumLength(200);
            RuleFor(command => command.Description).NotEmpty().MinimumLength(20).MaximumLength(8000);
            RuleFor(command => command.OpenPositions).GreaterThan(0);
            RuleFor(command => command.Currency).Length(3).When(command => command.Currency is not null);

            // Mirrors the ck_job_advertisements_salary_range check constraint, so the caller gets a
            // 400 with a field-level message instead of a database error.
            RuleFor(command => command.MaxSalary)
                .GreaterThanOrEqualTo(command => command.MinSalary)
                .When(command => command.MinSalary.HasValue && command.MaxSalary.HasValue)
                .WithMessage("Maksimum maaş, minimum maaştan küçük olamaz.");

            RuleFor(command => command.Deadline)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Son başvuru tarihi geçmiş bir tarih olamaz.");
        }
    }

    public sealed class UpdateJobAdvertisementCommandValidator : AbstractValidator<UpdateJobAdvertisementCommand>
    {
        public UpdateJobAdvertisementCommandValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.EmployerId).NotEmpty();
            RuleFor(command => command.Title).NotEmpty().MinimumLength(3).MaximumLength(200);
            RuleFor(command => command.JobPositionName).NotEmpty().MaximumLength(200);
            RuleFor(command => command.Description).NotEmpty().MinimumLength(20).MaximumLength(8000);
            RuleFor(command => command.OpenPositions).GreaterThan(0);

            RuleFor(command => command.MaxSalary)
                .GreaterThanOrEqualTo(command => command.MinSalary)
                .When(command => command.MinSalary.HasValue && command.MaxSalary.HasValue)
                .WithMessage("Maksimum maaş, minimum maaştan küçük olamaz.");
        }
    }

    public sealed class CreateJobApplicationCommandValidator : AbstractValidator<CreateJobApplicationCommand>
    {
        public CreateJobApplicationCommandValidator()
        {
            RuleFor(command => command.JobAdvertisementId).NotEmpty();
            RuleFor(command => command.JobSeekerId).NotEmpty();
            RuleFor(command => command.JobSeekerNote).MaximumLength(2000);
        }
    }

    public sealed class UpdateJobApplicationCommandValidator : AbstractValidator<UpdateJobApplicationCommand>
    {
        public UpdateJobApplicationCommandValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.EmployerId).NotEmpty();
            RuleFor(command => command.Status).IsInEnum();
            RuleFor(command => command.EmployerNote).MaximumLength(2000);
        }
    }

    public sealed class UpdateEmployerCommandValidator : AbstractValidator<UpdateEmployerCommand>
    {
        public UpdateEmployerCommandValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.CompanyName).NotEmpty().MinimumLength(2).MaximumLength(200);
            RuleFor(command => command.WebSite).MaximumLength(256);
            RuleFor(command => command.NumberOfEmployees).GreaterThan(0)
                .When(command => command.NumberOfEmployees.HasValue);
            RuleForEach(command => command.Departments).ChildRules(department =>
                department.RuleFor(input => input.Name).NotEmpty().MaximumLength(200));
        }
    }

    public sealed class UpdateJobSeekerCommandValidator : AbstractValidator<UpdateJobSeekerCommand>
    {
        public UpdateJobSeekerCommandValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.FirstName).NotEmpty().MinimumLength(2).MaximumLength(100);
            RuleFor(command => command.LastName).NotEmpty().MinimumLength(2).MaximumLength(100);
            RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
        }
    }

    public sealed class UpdateSystemStaffCommandValidator : AbstractValidator<UpdateSystemStaffCommand>
    {
        public UpdateSystemStaffCommandValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.FirstName).NotEmpty().MinimumLength(2).MaximumLength(100);
            RuleFor(command => command.LastName).NotEmpty().MinimumLength(2).MaximumLength(100);
            RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
        }
    }

    public sealed class CreateCvCommandValidator : AbstractValidator<CreateCvCommand>
    {
        public CreateCvCommandValidator()
        {
            RuleFor(command => command.JobSeekerId).NotEmpty();
            RuleFor(command => command.Information).MaximumLength(4000);
            RuleForEach(command => command.Educations).SetValidator(new EducationInputValidator());
            RuleForEach(command => command.JobExperiences).SetValidator(new JobExperienceInputValidator());
        }
    }

    public sealed class UpdateCvCommandValidator : AbstractValidator<UpdateCvCommand>
    {
        public UpdateCvCommandValidator()
        {
            RuleFor(command => command.JobSeekerId).NotEmpty();
            RuleFor(command => command.Information).MaximumLength(4000);
            RuleForEach(command => command.Educations).SetValidator(new EducationInputValidator());
            RuleForEach(command => command.JobExperiences).SetValidator(new JobExperienceInputValidator());
        }
    }

    internal sealed class EducationInputValidator : AbstractValidator<Common.Dtos.EducationInput>
    {
        public EducationInputValidator()
        {
            RuleFor(input => input.School).NotEmpty().MaximumLength(200);
            RuleFor(input => input.Major).NotEmpty().MaximumLength(200);

            // Now checkable at all: these were a string[] of years before, so no ordering rule was
            // expressible.
            RuleFor(input => input.EndYear)
                .GreaterThanOrEqualTo(input => input.StartYear)
                .When(input => input.StartYear.HasValue && input.EndYear.HasValue);
        }
    }

    internal sealed class JobExperienceInputValidator : AbstractValidator<Common.Dtos.JobExperienceInput>
    {
        public JobExperienceInputValidator()
        {
            RuleFor(input => input.CompanyName).NotEmpty().MaximumLength(200);
            RuleFor(input => input.Position).NotEmpty().MaximumLength(200);
            RuleFor(input => input.Description).MaximumLength(2000);

            RuleFor(input => input.EndYear)
                .GreaterThanOrEqualTo(input => input.StartYear)
                .When(input => input.StartYear.HasValue && input.EndYear.HasValue);
        }
    }
}
