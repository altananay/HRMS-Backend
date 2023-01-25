using Application.Features.SystemStaffs.Commands;
using Application.Results;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface ISystemStaffService
    {
        IDataResult<IQueryable<SystemStaff>> GetAll();
        Task<IResult> Add(SystemStaff employer);
        Task<IResult> Delete(string id);
        Task<IResult> UpdateAsync(UpdateSystemStaffCommand employer);
        IDataResult<SystemStaff> GetById(string id);
        IDataResult<SystemStaff> GetByEmail(string email);
    }
}