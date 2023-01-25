using Application.Abstractions;
using Application.Repositories;
using Application.Results;
using Domain.Entities;

namespace Persistence.Concretes
{
    public class UserManager : IUserService
    {
        private readonly IUserWriteRepository _userWriteRepository;
        private readonly IUserDeleteRepository _userDeleteRepository;
        private readonly IUserReadRepository _userReadRepository;

        public UserManager(IUserWriteRepository userWriteRepository, IUserDeleteRepository userDeleteRepository, IUserReadRepository userReadRepository)
        {
            _userWriteRepository = userWriteRepository;
            _userDeleteRepository = userDeleteRepository;
            _userReadRepository = userReadRepository;
        }

        public async Task<IResult> Add(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            await _userWriteRepository.AddAsync(user);
            return new SuccessResult();
        }

        public async Task<IResult> Delete(string id)
        {
            await _userDeleteRepository.Delete(id);
            return new SuccessResult();
        }

        public IDataResult<IQueryable<User>> GetAll()
        {
            return new SuccessDataResult<IQueryable<User>>(_userReadRepository.GetAll());
        }

        public IDataResult<User> GetById(string id)
        {
            return new SuccessDataResult<User>(_userReadRepository.GetById(id));
        }

        public async Task<IResult> Update(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _userWriteRepository.UpdateAsync(user);
            return new SuccessResult();
        }
    }
}