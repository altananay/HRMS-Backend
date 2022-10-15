using Domain.Common;

namespace Application.Repositories
{
    public interface IDeleteRepository<T> : IRepository<T> where T : BaseEntity
    {
        Task<bool> Delete(string id);
    }
}