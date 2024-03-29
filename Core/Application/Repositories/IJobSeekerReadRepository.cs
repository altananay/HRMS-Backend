﻿using Application.Utilities.Dtos;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IJobSeekerReadRepository : IReadRepository<JobSeeker>
    {
        IQueryable<GetAllJobSeekerDto> GetAllJobSeeker();
    }
}