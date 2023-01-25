using Application.Features.Cvs.Commands;
using Application.Results;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface ICVService
    {
        IDataResult<IQueryable<Cv>> GetAll();
        Task<IResult> Add(CreateCvCommand cv);
        Task<IResult> Delete(string id);
        Task<IResult> Update(UpdateCvCommand user);
        IDataResult<Cv> GetByJobSeekerId(string id);
    }
}