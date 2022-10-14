using Domain.Common;
using System.Linq.Expressions;

namespace Application.Repositories
{
    public interface IReadRepository<T> : IRepository<T> where T : BaseEntity
    {
        T GetById(string id);
        IQueryable<T> GetAll();
        T Get(Expression<Func<T, bool>> filter);
    }
}