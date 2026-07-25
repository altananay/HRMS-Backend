namespace Application.Abstractions.Storage
{
    /// <summary>A file to be stored, decoupled from ASP.NET's IFormFile.</summary>
    /// <remarks>
    /// The storage interfaces used to take <c>IFormFileCollection</c>, which is the reason
    /// Application had to reference the ASP.NET Core shared framework at all. Controllers now
    /// translate IFormFile into this record, so the HTTP type stays in the Presentation layer.
    /// </remarks>
    public sealed record FileUploadRequest(string FileName, string? ContentType, long Length, Stream Content);

    public sealed record StoredFile(string FileName, string StoragePath, string? ContentType, long SizeBytes);

    public interface IStorage
    {
        Task<IReadOnlyList<StoredFile>> UploadAsync(
            string containerName,
            IReadOnlyList<FileUploadRequest> files,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(string containerName, string fileName, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<string>> GetFilesAsync(string containerName, CancellationToken cancellationToken = default);

        Task<bool> HasFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default);
    }
}
