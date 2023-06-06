using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.CrossCuttingConcerns.Validation.Validators.Common;
using Application.CrossCuttingConcerns.Validation.Validators.Employers.Auth;
using Application.Dtos;
using Application.Features.Employers.Commands;
using Application.Repositories;
using Application.Results;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Persistence.Concretes
{
    public class EmployerManager : IEmployerService
    {
        private readonly IEmployerReadRepository _employerReadRepository;
        private readonly IEmployerDeleteRepository _employerDeleteRepository;
        private readonly IEmployerWriteRepository _employerWriteRepository;
        private readonly IUserService _userService;
        public EmployerManager(IEmployerReadRepository employerReadRepository, IEmployerDeleteRepository employerDeleteRepository, IEmployerWriteRepository employerWriteRepository, IUserService userService, ILogger<EmployerManager> logger)
        {
            _employerReadRepository = employerReadRepository;
            _employerDeleteRepository = employerDeleteRepository;
            _employerWriteRepository = employerWriteRepository;
            _userService = userService;
        }

        [ValidationAspect(typeof(EmployerValidator))]
        public async Task<IResult> Add(Employer employer)
        {
            var user = new User();
            await _userService.Add(user);
            employer.Id = user.Id;
            await _employerWriteRepository.AddAsync(employer);
            return new SuccessResult(Messages.EmployerAdded);
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public async Task<IResult> Delete(string id)
        {
            await _userService.Delete(id);
            await _employerDeleteRepository.Delete(id);
            return new SuccessResult(Messages.EmployerDeleted);

        }

        public IDataResult<IQueryable<Employer>> GetAll()
        {
            return new SuccessDataResult<IQueryable<Employer>>(_employerReadRepository.GetAll());
        }

        public IDataResult<IQueryable<Employer>> GetAllByHighestNumberOfEmployees()
        {
            return new SuccessDataResult<IQueryable<Employer>>(_employerReadRepository.GetAll().OrderBy(e => e.NumberOfEmployees));
        }

        public IDataResult<IQueryable<GetAllEmployerDto>> GetAllEmployer()
        {
            return new SuccessDataResult<IQueryable<GetAllEmployerDto>>(_employerReadRepository.GetAllEmployer());
        }

        public IDataResult<Employer> GetByEmail(string email)
        {
            return new SuccessDataResult<Employer>(_employerReadRepository.Get(u => u.Email == email));

        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<GetEmployerDto> GetByEmployerIdWithFields(string id)
        {
            return new SuccessDataResult<GetEmployerDto>(_employerReadRepository.GetByEmployerIdWithFields(id));
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<Employer> GetById(string id)
        {
            return new SuccessDataResult<Employer>(_employerReadRepository.Get(e => e.Id == id));
        }

        [ValidationAspect(typeof(EmployerValidator))]
        public async Task<IResult> Update(UpdateEmployerCommand employer)
        {
            var result = _employerReadRepository.Get(e => e.Email == employer.Email);
            var employerEntity = new Employer
            {
                Email = employer.Email,
                Claims = result.Claims,
                CompanyName = employer.CompanyName,
                CompanyPhone = employer.CompanyPhone,
                CreatedAt = result.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                WebSite = employer.WebSite,
                PasswordHash = result.PasswordHash,
                PasswordSalt = result.PasswordSalt,
                Status = result.Status,
                Id = result.Id,
                Description = employer.Description,
                Departments = employer.Departments,
                Sector = employer.Sector,
                NumberOfEmployees = employer.NumberOfEmployees
            };
            var result2 = await _employerWriteRepository.UpdateAsync(employerEntity);
            return new SuccessResult(Messages.EmployerUpdated);
        }
    }
}