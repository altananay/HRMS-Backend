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
        IResult Add(JobAdvertisement jobAdvertisement);
        IResult Delete(string id);
        IResult Update(JobAdvertisement jobAdvertisement);
        IDataResult<IQueryable<JobAdvertisement>> GetAll();
        IDataResult<JobAdvertisement> GetById(string id);
        IResult JobAdvertisementExists(string jobPosition);
    }
}