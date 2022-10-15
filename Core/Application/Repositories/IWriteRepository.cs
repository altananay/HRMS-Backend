using Domain.Common;

namespace Application.Repositories
{
    public interface IWriteRepository<T> : IRepository<T> where T : BaseEntity
    {
        Task<bool> Add(T entity);
        Task<bool> Update(T entity);
    }
}