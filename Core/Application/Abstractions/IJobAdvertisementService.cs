using Application.Features.JobAdvertisements.Commands;
using Application.Results;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IJobAdvertisementService
    {
        Task<IResult> Add(CreateJobAdvertisementCommand jobAdvertisement);
        Task<IResult> Delete(string id);
        Task<IResult> Update(UpdateJobAdvertisementCommand jobAdvertisement);
        IDataResult<IQueryable<JobAdvertisement>> GetAll();
        IDataResult<JobAdvertisement> GetById(string id);
        IResult JobAdvertisementExists(string jobPosition);
        IDataResult<IQueryable<JobAdvertisement>> GetAllByStatus(bool status);
        IDataResult<IQueryable<JobAdvertisement>> GetByEmployerId(string employerId);
        IDataResult<IQueryable<JobAdvertisement>> GetByEmployerIdWithStatus(string id, bool status);
    }
}