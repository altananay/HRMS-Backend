using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.Repositories;
using Application.Results;
using Application.Validators;
using Domain.Entities;

namespace Persistence.Concretes
{
    public class CvManager : ICVService
    {
        private readonly ICvWriteRepository _cvWriteRepository;
        private readonly ICvDeleteRepository _cvDeleteRepository;
        private readonly ICvReadRepository _cvReadRepository;

        public CvManager(ICvWriteRepository cvWriteRepository, ICvDeleteRepository cvDeleteRepository, ICvReadRepository cvReadRepository)
        {
            _cvWriteRepository = cvWriteRepository;
            _cvDeleteRepository = cvDeleteRepository;
            _cvReadRepository = cvReadRepository;
        }

        [ValidationAspect(typeof(CvValidator))]
        public IResult Add(Cv cv)
        {
            cv.CreatedAt = DateTime.UtcNow;
            _cvWriteRepository.AddAsync(cv);
            return new SuccessResult(Messages.CvAdded);
        }

        [ValidationAspect(typeof(DeleteValidator))]
        public IResult Delete(string id)
        {
            _cvDeleteRepository.Delete(id);
            return new SuccessResult(Messages.CvDeleted);
        }

        public IDataResult<IQueryable<Cv>> GetAll()
        {
            return new SuccessDataResult<IQueryable<Cv>>(_cvReadRepository.GetAll());
        }

        public IDataResult<Cv> GetByJobSeekerId(string id)
        {
            return new SuccessDataResult<Cv>(_cvReadRepository.Get(cv => cv.JobSeekerId == id));
        }

        public IResult Update(Cv cv)
        {
            _cvWriteRepository.UpdateAsync(cv);
            return new SuccessResult(Messages.CvUpdated);
        }
    }
}