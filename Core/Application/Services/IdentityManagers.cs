using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Common.Contracts;
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

        public async Task<IDataResult<PagedResult<EmployerResponse>>> GetPagedAsync(
            PageRequest page,
            bool orderByHeadcount = false,
            CancellationToken cancellationToken = default)
        {
            var result = await _employers.GetPagedAsync(page, orderByHeadcount, cancellationToken);

            return new SuccessDataResult<PagedResult<EmployerResponse>>(new PagedResult<EmployerResponse>(
                result.Items.Select(DomainMapper.ToResponse).ToList(), result.Page, result.PageSize, result.TotalCount));
        }

        public async Task<IDataResult<EmployerDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var employer = await _rules.EnsureEmployerExistsAsync(id, cancellationToken);
            return new SuccessDataResult<EmployerDetailResponse>(DomainMapper.ToDetailResponse(employer));
        }

        public async Task<IDataResult<EmployerResponse>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var employer = await _employers.GetByEmailAsync(email, cancellationToken)
                ?? throw new Common.Exceptions.NotFoundException(Messages.Employer.NotFound);

            return new SuccessDataResult<EmployerResponse>(DomainMapper.ToResponse(employer));
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

        public async Task<IDataResult<PagedResult<JobSeekerResponse>>> GetPagedAsync(
            PageRequest page,
            CancellationToken cancellationToken = default)
        {
            var result = await _jobSeekers.GetPagedAsync(page, cancellationToken);

            return new SuccessDataResult<PagedResult<JobSeekerResponse>>(new PagedResult<JobSeekerResponse>(
                result.Items.Select(DomainMapper.ToResponse).ToList(), result.Page, result.PageSize, result.TotalCount));
        }

        public async Task<IDataResult<JobSeekerResponse>> GetByIdAsync(Guid id, Guid requestedBy, CancellationToken cancellationToken = default)
        {
            await _access.EnsureCanReadAsync(id, requestedBy, cancellationToken);

            var jobSeeker = await _rules.EnsureJobSeekerExistsAsync(id, cancellationToken);
            return new SuccessDataResult<JobSeekerResponse>(DomainMapper.ToResponse(jobSeeker));
        }

        public async Task<IDataResult<JobSeekerResponse>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var jobSeeker = await _jobSeekers.GetByEmailAsync(email, cancellationToken)
                ?? throw new Common.Exceptions.NotFoundException(Messages.JobSeeker.NotFound);

            return new SuccessDataResult<JobSeekerResponse>(DomainMapper.ToResponse(jobSeeker));
        }

        public async Task<IResult> UpdateAsync(UpdateJobSeekerCommand command, CancellationToken cancellationToken = default)
        {
            var jobSeeker = await _rules.EnsureJobSeekerExistsAsync(command.Id, cancellationToken);

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

        public async Task<IDataResult<PagedResult<SystemStaffResponse>>> GetPagedAsync(
            PageRequest page,
            CancellationToken cancellationToken = default)
        {
            var result = await _systemStaff.GetPagedAsync(page, cancellationToken);

            return new SuccessDataResult<PagedResult<SystemStaffResponse>>(new PagedResult<SystemStaffResponse>(
                result.Items.Select(DomainMapper.ToResponse).ToList(), result.Page, result.PageSize, result.TotalCount));
        }

        public async Task<IDataResult<SystemStaffResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var staff = await _systemStaff.GetByIdAsync(id, cancellationToken)
                ?? throw new Common.Exceptions.NotFoundException(Messages.SystemStaff.NotFound);

            return new SuccessDataResult<SystemStaffResponse>(DomainMapper.ToResponse(staff));
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

        public async Task<IDataResult<PagedResult<UserSummaryResponse>>> GetPagedAsync(
            PageRequest page,
            CancellationToken cancellationToken = default)
        {
            var result = await _users.GetPagedAsync(page, cancellationToken);

            return new SuccessDataResult<PagedResult<UserSummaryResponse>>(new PagedResult<UserSummaryResponse>(
                result.Items.Select(DomainMapper.ToSummaryResponse).ToList(), result.Page, result.PageSize, result.TotalCount));
        }
    }
}
