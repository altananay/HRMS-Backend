using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Common.Dtos;
using Application.Common.Models;
using Application.Features.Employers.Commands;
using Application.Features.JobSeekers.Commands;
using Application.Features.SystemStaffs.Commands;
using Application.Mapping;
using Application.Results;
using Application.Rules;
using Application.Utilities.Constants;
using Domain.Entities;

namespace Application.Services
{
    public sealed class EmployerManager : IEmployerService
    {
        private readonly IEmployerRepository _employers;
        private readonly IUnitOfWork _unitOfWork;
        private readonly BusinessRules _rules;

        public EmployerManager(IEmployerRepository employers, IUnitOfWork unitOfWork, BusinessRules rules)
        {
            _employers = employers;
            _unitOfWork = unitOfWork;
            _rules = rules;
        }

        public async Task<IDataResult<PagedResult<EmployerDto>>> GetPagedAsync(
            PageRequest page,
            bool orderByHeadcount = false,
            CancellationToken cancellationToken = default)
        {
            var result = await _employers.GetPagedAsync(page, orderByHeadcount, cancellationToken);

            return new SuccessDataResult<PagedResult<EmployerDto>>(new PagedResult<EmployerDto>(
                result.Items.Select(DomainMapper.ToDto).ToList(), result.Page, result.PageSize, result.TotalCount));
        }

        public async Task<IDataResult<EmployerDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var employer = await _rules.EnsureEmployerExistsAsync(id, cancellationToken);
            return new SuccessDataResult<EmployerDetailDto>(DomainMapper.ToDetailDto(employer));
        }

        /// <remarks>
        /// Returns 404 when no employer matches rather than leaking whether an address is
        /// registered through a differently-shaped response.
        /// </remarks>
        public async Task<IDataResult<EmployerDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var employer = await _employers.GetByEmailAsync(email, cancellationToken)
                ?? throw new Common.Exceptions.NotFoundException(Messages.Employer.NotFound);

            return new SuccessDataResult<EmployerDto>(DomainMapper.ToDto(employer));
        }

        public async Task<IResult> UpdateAsync(UpdateEmployerCommand command, CancellationToken cancellationToken = default)
        {
            var employer = await _rules.EnsureEmployerExistsAsync(command.Id, cancellationToken);

            employer.CompanyName = command.CompanyName;
            employer.CompanyPhone = command.CompanyPhone;
            employer.WebSite = command.WebSite;
            employer.NumberOfEmployees = command.NumberOfEmployees;
            employer.Description = command.Description;
            employer.Sectors = command.Sectors;

            // Departments are replaced wholesale; cascade delete removes the detached rows.
            employer.Departments.Clear();
            foreach (var department in command.Departments)
            {
                employer.Departments.Add(new Department
                {
                    EmployerId = employer.Id,
                    Name = department.Name,
                    NumberOfEmployees = department.NumberOfEmployees
                });
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.Employer.Updated);
        }

        public async Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var employer = await _rules.EnsureEmployerExistsAsync(id, cancellationToken);

            // Soft delete via the auditing interceptor — the employer's advertisements and the
            // applications attached to them are hiring history and must survive.
            _employers.Remove(employer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.Employer.Deleted);
        }
    }

    public sealed class JobSeekerManager : IJobSeekerService
    {
        private readonly IJobSeekerRepository _jobSeekers;
        private readonly IUnitOfWork _unitOfWork;
        private readonly BusinessRules _rules;
        private readonly CandidateAccessPolicy _access;

        public JobSeekerManager(
            IJobSeekerRepository jobSeekers,
            IUnitOfWork unitOfWork,
            BusinessRules rules,
            CandidateAccessPolicy access)
        {
            _jobSeekers = jobSeekers;
            _unitOfWork = unitOfWork;
            _rules = rules;
            _access = access;
        }

        public async Task<IDataResult<PagedResult<JobSeekerDto>>> GetPagedAsync(
            PageRequest page,
            CancellationToken cancellationToken = default)
        {
            var result = await _jobSeekers.GetPagedAsync(page, cancellationToken);

            return new SuccessDataResult<PagedResult<JobSeekerDto>>(new PagedResult<JobSeekerDto>(
                result.Items.Select(DomainMapper.ToDto).ToList(), result.Page, result.PageSize, result.TotalCount));
        }

        public async Task<IDataResult<JobSeekerDto>> GetByIdAsync(Guid id, Guid requestedBy, CancellationToken cancellationToken = default)
        {
            // Same policy as the CV: the candidate, an employer holding an application from them, or
            // an admin. Without it any authenticated caller could walk the id space and collect every
            // seeker's email and date of birth.
            await _access.EnsureCanReadAsync(id, requestedBy, cancellationToken);

            var jobSeeker = await _rules.EnsureJobSeekerExistsAsync(id, cancellationToken);
            return new SuccessDataResult<JobSeekerDto>(DomainMapper.ToDto(jobSeeker));
        }

        public async Task<IDataResult<JobSeekerDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var jobSeeker = await _jobSeekers.GetByEmailAsync(email, cancellationToken)
                ?? throw new Common.Exceptions.NotFoundException(Messages.JobSeeker.NotFound);

            return new SuccessDataResult<JobSeekerDto>(DomainMapper.ToDto(jobSeeker));
        }

        public async Task<IResult> UpdateAsync(UpdateJobSeekerCommand command, CancellationToken cancellationToken = default)
        {
            var jobSeeker = await _rules.EnsureJobSeekerExistsAsync(command.Id, cancellationToken);

            // All four fields are applied. The old manager wrote only Email and silently discarded
            // everything else the caller sent, inside a try/catch that swallowed the reason.
            jobSeeker.FirstName = command.FirstName;
            jobSeeker.LastName = command.LastName;
            jobSeeker.Email = command.Email;
            jobSeeker.DateOfBirth = command.DateOfBirth;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.JobSeeker.Updated);
        }

        public async Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var jobSeeker = await _rules.EnsureJobSeekerExistsAsync(id, cancellationToken);

            // One soft delete, one SaveChanges. Previously this deleted from the users collection
            // and then the jobseekers collection as two independent Mongo calls with no transaction,
            // so a failure between them left the two permanently inconsistent.
            _jobSeekers.Remove(jobSeeker);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.JobSeeker.Deleted);
        }
    }

    public sealed class SystemStaffManager : ISystemStaffService
    {
        private readonly ISystemStaffRepository _systemStaff;
        private readonly IUnitOfWork _unitOfWork;

        public SystemStaffManager(ISystemStaffRepository systemStaff, IUnitOfWork unitOfWork)
        {
            _systemStaff = systemStaff;
            _unitOfWork = unitOfWork;
        }

        public async Task<IDataResult<PagedResult<SystemStaffDto>>> GetPagedAsync(
            PageRequest page,
            CancellationToken cancellationToken = default)
        {
            var result = await _systemStaff.GetPagedAsync(page, cancellationToken);

            return new SuccessDataResult<PagedResult<SystemStaffDto>>(new PagedResult<SystemStaffDto>(
                result.Items.Select(DomainMapper.ToDto).ToList(), result.Page, result.PageSize, result.TotalCount));
        }

        public async Task<IDataResult<SystemStaffDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var staff = await _systemStaff.GetByIdAsync(id, cancellationToken)
                ?? throw new Common.Exceptions.NotFoundException(Messages.SystemStaff.NotFound);

            return new SuccessDataResult<SystemStaffDto>(DomainMapper.ToDto(staff));
        }

        public async Task<IResult> UpdateAsync(UpdateSystemStaffCommand command, CancellationToken cancellationToken = default)
        {
            var staff = await _systemStaff.GetByIdAsync(command.Id, cancellationToken)
                ?? throw new Common.Exceptions.NotFoundException(Messages.SystemStaff.NotFound);

            staff.FirstName = command.FirstName;
            staff.LastName = command.LastName;
            staff.Email = command.Email;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.SystemStaff.Updated);
        }

        public async Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var staff = await _systemStaff.GetByIdAsync(id, cancellationToken)
                ?? throw new Common.Exceptions.NotFoundException(Messages.SystemStaff.NotFound);

            _systemStaff.Remove(staff);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.SystemStaff.Deleted);
        }
    }

    public sealed class UserManager : IUserService
    {
        private readonly IUserRepository _users;

        public UserManager(IUserRepository users) => _users = users;

        public async Task<IDataResult<PagedResult<UserSummaryDto>>> GetPagedAsync(
            PageRequest page,
            CancellationToken cancellationToken = default)
        {
            var result = await _users.GetPagedAsync(page, cancellationToken);

            return new SuccessDataResult<PagedResult<UserSummaryDto>>(new PagedResult<UserSummaryDto>(
                result.Items.Select(DomainMapper.ToSummaryDto).ToList(), result.Page, result.PageSize, result.TotalCount));
        }
    }
}
