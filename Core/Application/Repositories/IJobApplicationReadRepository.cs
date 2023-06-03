using Application.Dtos;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IJobApplicationReadRepository : IReadRepository<JobApplication>
    {
        GetJobApplicationResultDto GetResultById(string id);
    }
}