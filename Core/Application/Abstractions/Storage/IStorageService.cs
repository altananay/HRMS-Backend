using Domain.Enums;

namespace Application.Abstractions.Storage
{
    public interface IStorageService : IStorage
    {
        /// <summary>Which provider is active, recorded on each stored CvFile row.</summary>
        StorageProvider Provider { get; }
    }
}
