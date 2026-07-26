namespace Application.Abstractions.Storage
{
    public sealed record FileUploadRequest(string FileName, string? ContentType, long Length, Stream Content);

    public sealed record StoredFile(string FileName, string StoragePath, string? ContentType, long SizeBytes);

    public interface IStorage
    {
        Task<IReadOnlyList<StoredFile>> UploadAsync(
            string containerName,
            IReadOnlyList<FileUploadRequest> files,
            CancellationToken cancellationToken = default);

        Task<Stream?> OpenReadAsync(string containerName, string fileName, CancellationToken cancellationToken = default);

        Task DeleteAsync(string containerName, string fileName, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<string>> GetFilesAsync(string containerName, CancellationToken cancellationToken = default);

        Task<bool> HasFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default);
    }
}
