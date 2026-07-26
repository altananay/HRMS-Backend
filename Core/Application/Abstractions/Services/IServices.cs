using Application.Common.Contracts;
using Application.Common.Models;
using Application.Features.Auth.Commands;
using Application.Features.Contacts.Commands;
using Application.Features.Cvs.Commands;
using Application.Features.Employers.Commands;
using Application.Features.JobAdvertisements.Commands;
using Application.Features.JobApplications.Commands;
using Application.Features.JobPositions.Commands;
using Application.Features.JobSeekers.Commands;
using Application.Features.SystemStaffs.Commands;
using Application.Results;
using Domain.Enums;

namespace Application.Abstractions.Services
{
    public interface IAuthService
    {
        Task<IDataResult<AuthResponse>> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default);

        Task<IDataResult<AuthResponse>> RegisterJobSeekerAsync(
            RegisterJobSeekerCommand command, CancellationToken cancellationToken = default);

        Task<IDataResult<AuthResponse>> RegisterEmployerAsync(
            RegisterEmployerCommand command, CancellationToken cancellationToken = default);

        Task<IDataResult<CreatedResponse>> RegisterSystemStaffAsync(
            RegisterSystemStaffCommand command, CancellationToken cancellationToken = default);

        Task<IDataResult<AuthResponse>> RefreshAsync(
            RefreshTokenCommand command, CancellationToken cancellationToken = default);

        Task<IResult> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);

        Task<IResult> LogoutAllAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<IResult> ChangePasswordAsync(
            ChangePasswordCommand command, CancellationToken cancellationToken = default);

        Task<IDataResult<AuthenticatedUserResponse>> GetCurrentUserAsync(
            Guid userId, CancellationToken cancellationToken = default);
    }

    public interface IContactService
    {
        Task<IDataResult<PagedResult<ContactResponse>>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
        Task<IDataResult<ContactResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IDataResult<CreatedResponse>> AddAsync(CreateContactCommand command, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateContactCommand command, CancellationToken cancellationToken = default);
        Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public interface IJobPositionService
    {
        Task<IDataResult<PagedResult<JobPositionResponse>>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
        Task<IDataResult<JobPositionResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IDataResult<CreatedResponse>> AddAsync(CreateJobPositionCommand command, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateJobPositionCommand command, CancellationToken cancellationToken = default);
        Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public interface IEmployerService
    {
        Task<IDataResult<PagedResult<EmployerResponse>>> GetPagedAsync(PageRequest page, bool orderByHeadcount = false, CancellationToken cancellationToken = default);
        Task<IDataResult<EmployerDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IDataResult<EmployerResponse>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateEmployerCommand command, CancellationToken cancellationToken = default);
        Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public interface IJobSeekerService
    {
        Task<IDataResult<PagedResult<JobSeekerResponse>>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);

        Task<IDataResult<JobSeekerResponse>> GetByIdAsync(Guid id, Guid requestedBy, CancellationToken cancellationToken = default);

        Task<IDataResult<JobSeekerResponse>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateJobSeekerCommand command, CancellationToken cancellationToken = default);
        Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public interface ISystemStaffService
    {
        Task<IDataResult<PagedResult<SystemStaffResponse>>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
        Task<IDataResult<SystemStaffResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateSystemStaffCommand command, CancellationToken cancellationToken = default);
        Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public interface IUserService
    {
        Task<IDataResult<PagedResult<UserSummaryResponse>>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
    }

    public interface ICvService
    {
        Task<IDataResult<PagedResult<CvResponse>>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);

        Task<IDataResult<CvResponse>> GetByJobSeekerIdAsync(Guid jobSeekerId, Guid requestedBy, CancellationToken cancellationToken = default);

        Task<IDataResult<CreatedResponse>> AddAsync(CreateCvCommand command, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateCvCommand command, CancellationToken cancellationToken = default);

        Task<IResult> DeleteAsync(Guid id, Guid requestedBy, CancellationToken cancellationToken = default);
    }

    public interface IJobAdvertisementService
    {
        Task<IDataResult<PagedResult<JobAdvertisementResponse>>> GetPagedAsync(
            PageRequest page, Guid? employerId = null, bool? isActive = null,
            bool orderByHighestSalary = false, CancellationToken cancellationToken = default);

        Task<IDataResult<JobAdvertisementResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IDataResult<CreatedResponse>> AddAsync(CreateJobAdvertisementCommand command, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateJobAdvertisementCommand command, CancellationToken cancellationToken = default);

        Task<IResult> DeleteAsync(Guid id, Guid employerId, CancellationToken cancellationToken = default);
    }

    public sealed record CvFileDownload(Stream Content, string FileName, string ContentType);

    public interface ICvFileService
    {
        Task<IDataResult<IReadOnlyList<CvFileResponse>>> UploadAsync(
            UploadCvFileCommand command, CancellationToken cancellationToken = default);

        Task<CvFileDownload> DownloadAsync(
            Guid fileId, Guid requestedBy, CancellationToken cancellationToken = default);

        Task<IResult> DeleteAsync(Guid fileId, Guid requestedBy, CancellationToken cancellationToken = default);
    }

    public interface IJobApplicationService
    {
        Task<IDataResult<PagedResult<JobApplicationResponse>>> GetPagedAsync(
            PageRequest page, Guid? employerId = null, Guid? jobSeekerId = null,
            JobApplicationStatus? status = null, CancellationToken cancellationToken = default);

        Task<IDataResult<JobApplicationResponse>> GetByIdAsync(Guid id, Guid requestedBy, CancellationToken cancellationToken = default);

        Task<IDataResult<CreatedResponse>> AddAsync(CreateJobApplicationCommand command, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateJobApplicationCommand command, CancellationToken cancellationToken = default);
        Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
