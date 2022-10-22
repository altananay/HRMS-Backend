using Application.Results;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface ICVService
    {
        IDataResult<IQueryable<Cv>> GetAll();
        IResult Add(Cv cv);
        IResult Delete(string id);
        IResult Update(Cv user);
        IDataResult<Cv> GetByJobSeekerId(string id);
    }
}