using Application.Abstractions;
using Application.Aspects;
using Application.CrossCuttingConcerns.Validation.Validators.Common;
using Application.CrossCuttingConcerns.Validation.Validators.Employers.Auth;
using Application.Features.Employers.Commands;
using Application.Repositories;
using Application.Results;
using Application.Utilities.Constants;
using Application.Utilities.Dtos;
using AutoMapper;
using Domain.Entities;
using Persistence.Rules;

namespace Persistence.Concretes
{
    public class EmployerManager : IEmployerService
    {
        private readonly IEmployerReadRepository _employerReadRepository;
        private readonly IEmployerDeleteRepository _employerDeleteRepository;
        private readonly IEmployerWriteRepository _employerWriteRepository;
        private readonly IUserService _userService;
        private readonly EmployerBusinessRules _rules;
        private readonly IMapper _mapper;

        public EmployerManager(IEmployerReadRepository employerReadRepository, IEmployerDeleteRepository employerDeleteRepository, IEmployerWriteRepository employerWriteRepository, IUserService userService, EmployerBusinessRules rules, IMapper mapper)
        {
            _employerReadRepository = employerReadRepository;
            _employerDeleteRepository = employerDeleteRepository;
            _employerWriteRepository = employerWriteRepository;
            _userService = userService;
            _rules = rules;
            _mapper = mapper;
        }

        [ValidationAspect(typeof(EmployerValidator))]
        public async Task<IResult> Add(Employer employer)
        {
            var user = new User();
            await _userService.Add(user);
            employer.Id = user.Id;
            await _employerWriteRepository.AddAsync(employer);
            return new SuccessResult(Messages.Employer.EmployerAdded);
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public async Task<IResult> Delete(string id)
        {
            _rules.CheckIfEmployerExists(id);
            await _employerDeleteRepository.Delete(id);
            await _userService.Delete(id);
            return new SuccessResult(Messages.Employer.EmployerDeleted);

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
            _rules.CheckIfEmployerExists(id);
            return new SuccessDataResult<GetEmployerDto>(_employerReadRepository.GetByEmployerIdWithFields(id));
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<Employer> GetById(string id)
        {
            _rules.CheckIfEmployerExists(id);
            return new SuccessDataResult<Employer>(_employerReadRepository.Get(e => e.Id == id));
        }

        [ValidationAspect(typeof(EmployerValidator))]
        public async Task<IResult> Update(UpdateEmployerCommand employer)
        {
            _rules.CheckIfEmployerExistsByEmail(employer.Email);
            var result = _employerReadRepository.Get(e => e.Email == employer.Email);
            Employer employerEntity = _mapper.Map<Employer>(employer);
            employerEntity.Claims = result.Claims;
            employerEntity.CreatedAt = result.CreatedAt;
            employerEntity.UpdatedAt = DateTime.UtcNow;
            employerEntity.PasswordHash = result.PasswordHash;
            employerEntity.PasswordSalt = result.PasswordSalt;
            employerEntity.Status = result.Status;
            employerEntity.Id = result.Id;
            await _employerWriteRepository.UpdateAsync(employerEntity);
            return new SuccessResult(Messages.Employer.EmployerUpdated);
        }
    }
}