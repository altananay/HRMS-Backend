using Application.Common.Dtos;
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
    /// <summary>
    /// Registration, sign-in, token lifecycle and password management for all three actor types.
    /// </summary>
    /// <remarks>
    /// One service replacing <c>AuthManager</c>, <c>EmployerAuthManager</c> and
    /// <c>SystemStaffAuthManager</c>. Those three duplicated the same flow with slightly different
    /// bugs each: the SystemStaff one alone null-checked the result wrapper instead of its
    /// <c>.Data</c>, passed a MediatR command where an <c>IValidator</c> was expected, and called a
    /// method guarded by <c>[SecuredOperation("admin")]</c> — so signing in as an administrator
    /// required already being one.
    /// </remarks>
    public interface IAuthService
    {
        Task<IDataResult<AuthResponse>> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default);

        Task<IDataResult<AuthResponse>> RegisterJobSeekerAsync(
            RegisterJobSeekerCommand command, CancellationToken cancellationToken = default);

        Task<IDataResult<AuthResponse>> RegisterEmployerAsync(
            RegisterEmployerCommand command, CancellationToken cancellationToken = default);

        Task<IResult> RegisterSystemStaffAsync(
            RegisterSystemStaffCommand command, CancellationToken cancellationToken = default);

        Task<IDataResult<AuthResponse>> RefreshAsync(
            RefreshTokenCommand command, CancellationToken cancellationToken = default);

        Task<IResult> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);

        Task<IResult> LogoutAllAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<IResult> ChangePasswordAsync(
            ChangePasswordCommand command, CancellationToken cancellationToken = default);

        Task<IDataResult<AuthenticatedUserDto>> GetCurrentUserAsync(
            Guid userId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Business-logic contracts, implemented by the managers in <c>Application/Services</c>.
    /// </summary>
    /// <remarks>
    /// The implementations moved out of <c>Persistence/Concretes</c>. Keeping business logic in the
    /// same project as the database provider meant nothing structurally stopped a manager from
    /// reaching for the provider directly — that boundary was enforced only by a convention written
    /// in a markdown file. Now the compiler enforces it: Application cannot see Npgsql at all.
    ///
    /// Every method is async and takes a CancellationToken. The old interfaces had synchronous reads
    /// returning <c>IQueryable&lt;T&gt;</c>, which meant the query was executed by the JSON
    /// serializer in the WebAPI layer, outside any error handling.
    /// </remarks>
    public interface IContactService
    {
        Task<IDataResult<PagedResult<ContactDto>>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
        Task<IDataResult<ContactDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IResult> AddAsync(CreateContactCommand command, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateContactCommand command, CancellationToken cancellationToken = default);
        Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public interface IJobPositionService
    {
        Task<IDataResult<PagedResult<JobPositionDto>>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
        Task<IDataResult<JobPositionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IResult> AddAsync(CreateJobPositionCommand command, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateJobPositionCommand command, CancellationToken cancellationToken = default);
        Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public interface IEmployerService
    {
        Task<IDataResult<PagedResult<EmployerDto>>> GetPagedAsync(PageRequest page, bool orderByHeadcount = false, CancellationToken cancellationToken = default);
        Task<IDataResult<EmployerDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IDataResult<EmployerDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateEmployerCommand command, CancellationToken cancellationToken = default);
        Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public interface IJobSeekerService
    {
        Task<IDataResult<PagedResult<JobSeekerDto>>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
        Task<IDataResult<JobSeekerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IDataResult<JobSeekerDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateJobSeekerCommand command, CancellationToken cancellationToken = default);
        Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public interface ISystemStaffService
    {
        Task<IDataResult<PagedResult<SystemStaffDto>>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
        Task<IDataResult<SystemStaffDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateSystemStaffCommand command, CancellationToken cancellationToken = default);
        Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public interface IUserService
    {
        Task<IDataResult<PagedResult<UserSummaryDto>>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
    }

    public interface ICvService
    {
        Task<IDataResult<PagedResult<CvDto>>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
        Task<IDataResult<CvDto>> GetByJobSeekerIdAsync(Guid jobSeekerId, CancellationToken cancellationToken = default);
        Task<IResult> AddAsync(CreateCvCommand command, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateCvCommand command, CancellationToken cancellationToken = default);
        Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public interface IJobAdvertisementService
    {
        Task<IDataResult<PagedResult<JobAdvertisementDto>>> GetPagedAsync(
            PageRequest page, Guid? employerId = null, bool? isActive = null,
            bool orderByHighestSalary = false, CancellationToken cancellationToken = default);

        Task<IDataResult<JobAdvertisementDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IResult> AddAsync(CreateJobAdvertisementCommand command, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateJobAdvertisementCommand command, CancellationToken cancellationToken = default);
        Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public interface IJobApplicationService
    {
        Task<IDataResult<PagedResult<JobApplicationDto>>> GetPagedAsync(
            PageRequest page, Guid? employerId = null, Guid? jobSeekerId = null,
            JobApplicationStatus? status = null, CancellationToken cancellationToken = default);

        Task<IDataResult<JobApplicationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IResult> AddAsync(CreateJobApplicationCommand command, CancellationToken cancellationToken = default);
        Task<IResult> UpdateAsync(UpdateJobApplicationCommand command, CancellationToken cancellationToken = default);
        Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
