using Domain.Common;
using MongoDB.Driver;

namespace Application.Repositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        IMongoCollection<T> collection { get; }
    }
}