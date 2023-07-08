using Application.Abstractions;
using Application.Aspects;
using Application.CrossCuttingConcerns.Validation.Validators.Common;
using Application.CrossCuttingConcerns.Validation.Validators.SystemStaffs;
using Application.Features.SystemStaffs.Commands;
using Application.Repositories;
using Application.Results;
using Application.Utilities.Constants;
using Domain.Entities;
using Persistence.Rules;

namespace Persistence.Concretes
{
    public class SystemStaffManager : ISystemStaffService
    {
        private readonly ISystemStaffDeleteRepository _systemStaffDeleteRepository;
        private readonly ISystemStaffReadRepository _systemStaffReadRepository;
        private readonly ISystemStaffWriteRepository _systemStaffWriteRepository;
        private readonly IUserService _userService;
        private readonly SystemStaffBusinessRules _systemStaffBusinessRules;

        public SystemStaffManager(ISystemStaffDeleteRepository systemStaffDeleteRepository, ISystemStaffReadRepository systemStaffReadRepository, ISystemStaffWriteRepository systemStaffWriteRepository, IUserService userService, SystemStaffBusinessRules systemStaffBusinessRules)
        {
            _systemStaffDeleteRepository = systemStaffDeleteRepository;
            _systemStaffReadRepository = systemStaffReadRepository;
            _systemStaffWriteRepository = systemStaffWriteRepository;
            _userService = userService;
            _systemStaffBusinessRules = systemStaffBusinessRules;
        }

        [SecuredOperation("admin")]
        [ValidationAspect(typeof(SystemStaffValidator))]
        public async Task<IResult> Add(SystemStaff systemStaff)
        {
            var user = new User();
            await _userService.Add(user);
            systemStaff.Id = user.Id;
            await _systemStaffWriteRepository.AddAsync(systemStaff);
            return new SuccessResult(Messages.SystemStaff.SystemStaffAdded);
        }

        [ValidationAspect(typeof(ObjectIdValidator))]
        [SecuredOperation("admin")]
        public async Task<IResult> Delete(string id)
        {
            _systemStaffBusinessRules.CheckIfSystemStaffExists(id);
            await _userService.Delete(id);
            await _systemStaffDeleteRepository.Delete(id);
            return new SuccessResult(Messages.SystemStaff.SystemStaffDeleted);
        }

        [SecuredOperation("admin")]
        public IDataResult<IQueryable<SystemStaff>> GetAll()
        {
            return new SuccessDataResult<IQueryable<SystemStaff>>(_systemStaffReadRepository.GetAll());
        }

        [SecuredOperation("admin")]
        public IDataResult<SystemStaff> GetByEmail(string email)
        {
            _systemStaffBusinessRules.CheckIfSystemStaffExistsByEmail(email);
            return new SuccessDataResult<SystemStaff>(_systemStaffReadRepository.Get(ss => ss.Email == email));
        }

        [SecuredOperation("admin")]
        [ValidationAspect(typeof(ObjectIdValidator))]
        public IDataResult<SystemStaff> GetById(string id)
        {
            _systemStaffBusinessRules.CheckIfSystemStaffExists(id);
            return new SuccessDataResult<SystemStaff>(_systemStaffReadRepository.Get(e => e.Id == id));
        }

        [SecuredOperation("admin")]
        [ValidationAspect(typeof(UpdateSystemStaffValidator))]
        public async Task<IResult> UpdateAsync(UpdateSystemStaffCommand systemStaff)
        {
            _systemStaffBusinessRules.CheckIfSystemStaffExists(systemStaff.Id);
            var result = _systemStaffReadRepository.GetById(systemStaff.Id);
            var entity = new SystemStaff
            {
                Email = systemStaff.Email,
                Claims = systemStaff.Claims,
                FirstName = systemStaff.FirstName,
                LastName = systemStaff.LastName,
                Status = systemStaff.Status,
                CreatedAt = result.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = result.PasswordHash,
                PasswordSalt = result.PasswordSalt,
                Id = result.Id,
            };
            await _systemStaffWriteRepository.UpdateAsync(entity);
            return new SuccessResult(Messages.SystemStaff.SystemStaffUpdated);
        }
    }
}