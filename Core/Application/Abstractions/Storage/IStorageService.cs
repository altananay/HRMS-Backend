using Domain.Enums;

namespace Application.Abstractions.Storage
{
    public interface IStorageService : IStorage
    {
        StorageProvider Provider { get; }
    }
}
