using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.Dtos;
using Application.Repositories;
using Application.Results;
using Application.Utilities.Security.Hashing;
using Application.Validators;
using Domain.Entities;

namespace Persistence.Concretes
{
    public class SystemStaffManager : ISystemStaffService
    {
        private readonly ISystemStaffDeleteRepository _systemStaffDeleteRepository;
        private readonly ISystemStaffReadRepository _systemStaffReadRepository;
        private readonly ISystemStaffWriteRepository _systemStaffWriteRepository;
        private readonly IUserService _userService;

        public SystemStaffManager(ISystemStaffDeleteRepository systemStaffDeleteRepository, ISystemStaffReadRepository systemStaffReadRepository, ISystemStaffWriteRepository systemStaffWriteRepository, IUserService userService)
        {
            _systemStaffDeleteRepository = systemStaffDeleteRepository;
            _systemStaffReadRepository = systemStaffReadRepository;
            _systemStaffWriteRepository = systemStaffWriteRepository;
            _userService = userService;
        }

        [SecuredOperation("admin")]
        public IResult Add(SystemStaff systemStaff)
        {
            var user = new User();
            _userService.Add(user);
            systemStaff.Id = user.Id;
            _systemStaffWriteRepository.Add(systemStaff);
            return new SuccessResult(Messages.SystemStaffAdded);
        }

        [ValidationAspect(typeof(DeleteValidator))]
        [SecuredOperation("admin")]
        public IResult Delete(string id)
        {
            _userService.Delete(id);
            _systemStaffDeleteRepository.Delete(id);
            return new SuccessResult(Messages.SystemStaffDeleted);
        }

        [SecuredOperation("admin")]
        public IDataResult<IQueryable<SystemStaff>> GetAll()
        {
            return new SuccessDataResult<IQueryable<SystemStaff>>(_systemStaffReadRepository.GetAll());
        }

        public IDataResult<SystemStaff> GetByEmail(string email)
        {
            return new SuccessDataResult<SystemStaff>(_systemStaffReadRepository.Get(ss => ss.Email == email));
        }

        [SecuredOperation("admin")]
        public IDataResult<SystemStaff> GetById(string id)
        {
            return new SuccessDataResult<SystemStaff>(_systemStaffReadRepository.Get(e => e.Id == id));
        }

        [SecuredOperation("admin")]
        public IResult Update(SystemStaffForRegisterDto systemStaff)
        {
            var result = _systemStaffReadRepository.Get(e => e.Email == systemStaff.Email);
            var entity = new SystemStaff
            {
                Email = systemStaff.Email,
                Claims = result.Claims,
                FirstName = systemStaff.FirstName,
                LastName = systemStaff.LastName,
                Status = result.Status,
                CreatedAt = result.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = result.PasswordHash,
                PasswordSalt = result.PasswordSalt,
            };
            _systemStaffWriteRepository.Update(entity);
            return new SuccessResult(Messages.SystemStaffUpdated);
        }
    }
}