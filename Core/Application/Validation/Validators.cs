using Application.Features.Auth.Commands;
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
            RuleForEach(command => command.Educations).SetValidator(new EducationRequestValidator());
            RuleForEach(command => command.JobExperiences).SetValidator(new JobExperienceRequestValidator());
        }
    }

    public sealed class UpdateCvCommandValidator : AbstractValidator<UpdateCvCommand>
    {
        public UpdateCvCommandValidator()
        {
            RuleFor(command => command.JobSeekerId).NotEmpty();
            RuleFor(command => command.Information).MaximumLength(4000);
            RuleForEach(command => command.Educations).SetValidator(new EducationRequestValidator());
            RuleForEach(command => command.JobExperiences).SetValidator(new JobExperienceRequestValidator());
        }
    }

    internal sealed class EducationRequestValidator : AbstractValidator<Common.Contracts.EducationRequest>
    {
        public EducationRequestValidator()
        {
            RuleFor(input => input.School).NotEmpty().MaximumLength(200);
            RuleFor(input => input.Major).NotEmpty().MaximumLength(200);

            RuleFor(input => input.EndYear)
                .GreaterThanOrEqualTo(input => input.StartYear)
                .When(input => input.StartYear.HasValue && input.EndYear.HasValue);
        }
    }

    internal sealed class JobExperienceRequestValidator : AbstractValidator<Common.Contracts.JobExperienceRequest>
    {
        public JobExperienceRequestValidator()
        {
            RuleFor(input => input.CompanyName).NotEmpty().MaximumLength(200);
            RuleFor(input => input.Position).NotEmpty().MaximumLength(200);
            RuleFor(input => input.Description).MaximumLength(2000);

            RuleFor(input => input.EndYear)
                .GreaterThanOrEqualTo(input => input.StartYear)
                .When(input => input.StartYear.HasValue && input.EndYear.HasValue);
        }
    }

    public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);

            // Presence only, never NewPassword(): a length rule here would disclose the policy to an
            // anonymous caller and lock out passwords that predate it.
            RuleFor(command => command.Password).NotEmpty();
        }
    }

    // Applies only where a password is set (registration, new password on change), never where one
    // is checked.
    internal static class PasswordPolicy
    {
        public const int MinimumLength = 5;

        public static IRuleBuilderOptions<T, string> NewPassword<T>(this IRuleBuilder<T, string> rule)
            => rule.NotEmpty().MinimumLength(MinimumLength);
    }

    public sealed class RegisterJobSeekerCommandValidator : AbstractValidator<RegisterJobSeekerCommand>
    {
        public RegisterJobSeekerCommandValidator()
        {
            RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(command => command.Password).NewPassword();
            RuleFor(command => command.FirstName).NotEmpty().MinimumLength(2).MaximumLength(100);
            RuleFor(command => command.LastName).NotEmpty().MinimumLength(2).MaximumLength(100);

            RuleFor(command => command.NationalId!)
                .Matches("^[0-9]{11}$")
                .When(command => !string.IsNullOrWhiteSpace(command.NationalId));
        }
    }

    public sealed class RegisterEmployerCommandValidator : AbstractValidator<RegisterEmployerCommand>
    {
        public RegisterEmployerCommandValidator()
        {
            RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(command => command.Password).NewPassword();
            RuleFor(command => command.CompanyName).NotEmpty().MinimumLength(2).MaximumLength(200);
            RuleFor(command => command.CompanyPhone).MaximumLength(32);
            RuleFor(command => command.WebSite).MaximumLength(256);
            RuleFor(command => command.Description).MaximumLength(4000);

            RuleFor(command => command.NumberOfEmployees).GreaterThan(0)
                .When(command => command.NumberOfEmployees.HasValue);

            RuleForEach(command => command.Sectors).NotEmpty().MaximumLength(100);
        }
    }

    public sealed class RegisterSystemStaffCommandValidator : AbstractValidator<RegisterSystemStaffCommand>
    {
        public RegisterSystemStaffCommandValidator()
        {
            RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(command => command.Password).NewPassword();
            RuleFor(command => command.FirstName).NotEmpty().MinimumLength(2).MaximumLength(100);
            RuleFor(command => command.LastName).NotEmpty().MinimumLength(2).MaximumLength(100);
        }
    }

    public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
            => RuleFor(command => command.RefreshToken).NotEmpty();
    }

    public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
            => RuleFor(command => command.RefreshToken).NotEmpty();
    }

    public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        // Structural only. Nothing here may depend on whether the address exists — the endpoint's
        // whole point is that a caller cannot tell.
        public ForgotPasswordCommandValidator()
            => RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }

    public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(command => command.Token).NotEmpty();
            RuleFor(command => command.NewPassword).NewPassword();
        }
    }

    public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(command => command.UserId).NotEmpty();

            RuleFor(command => command.CurrentPassword).NotEmpty();

            RuleFor(command => command.NewPassword).NewPassword();
        }
    }
}
