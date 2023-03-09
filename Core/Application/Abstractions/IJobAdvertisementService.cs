using Application.Features.JobAdvertisements.Commands;
using Application.Results;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}