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

        public IResult Add(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            _userWriteRepository.Add(user);
            return new SuccessResult();
        }

        public IResult Delete(string id)
        {
            _userDeleteRepository.Delete(id);
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

        public IResult Update(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _userWriteRepository.Update(user);
            return new SuccessResult();
        }
    }
}