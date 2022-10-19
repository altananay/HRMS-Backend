using Application.Dtos;
using Application.Results;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface ISystemStaffService
    {
        IDataResult<IQueryable<SystemStaff>> GetAll();
        IResult Add(SystemStaff employer);
        IResult Delete(string id);
        IResult Update(SystemStaffForRegisterDto employer);
        IDataResult<SystemStaff> GetById(string id);
        IDataResult<SystemStaff> GetByEmail(string email);
    }
}