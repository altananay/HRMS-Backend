using Application.Abstractions.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Storage
{
    /// <summary>
    /// Stores uploads in Azure Blob Storage.
    /// </summary>
    /// <remarks>
    /// Three defects from the previous version are fixed here.
    ///
    /// <b>Uploads are no longer world-readable.</b> It called
    /// <c>SetAccessPolicyAsync(PublicAccessType.BlobContainer)</c> on every upload, which made every
    /// CV in the container anonymously downloadable by anyone who could guess or enumerate a URL.
    /// Containers are now created private; a time-limited SAS URI is the way to hand out a file.
    ///
    /// <b>Filenames are correct.</b> It used <c>file.Name</c> — the multipart form field name, which
    /// is the same for every file in a request — instead of <c>file.FileName</c>, so a batch upload
    /// collapsed onto one blob.
    ///
    /// <b>No shared mutable state.</b> <c>_blobContainerClient</c> was an instance field reassigned
    /// by each method on a service resolved per scope, so concurrent requests could observe each
    /// other's container. Clients are now resolved locally from a singleton BlobServiceClient.
    /// </remarks>
    public sealed class AzureStorage : IStorage
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureStorage(IConfiguration configuration)
        {
            var connectionString = configuration["Storage:Azure:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Storage:Azure:ConnectionString is not configured but Storage:Provider is set to Azure. " +
                    "Use Storage:Provider=Local for local development.");
            }

            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<IReadOnlyList<StoredFile>> UploadAsync(
            string containerName,
            IReadOnlyList<FileUploadRequest> files,
            CancellationToken cancellationToken = default)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);

            // PublicAccessType.None — private by default.
            await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            var stored = new List<StoredFile>(files.Count);

            foreach (var file in files)
            {
                var blobName = $"{Guid.CreateVersion7()}{Path.GetExtension(file.FileName)}";
                var blob = container.GetBlobClient(blobName);

                await blob.UploadAsync(
                    file.Content,
                    new BlobHttpHeaders { ContentType = file.ContentType },
                    cancellationToken: cancellationToken);

                stored.Add(new StoredFile(file.FileName, $"{containerName}/{blobName}", file.ContentType, file.Length));
            }

            return stored;
        }

        public async Task DeleteAsync(string containerName, string fileName, CancellationToken cancellationToken = default)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            await container.DeleteBlobIfExistsAsync(fileName, cancellationToken: cancellationToken);
        }

        public async Task<IReadOnlyList<string>> GetFilesAsync(
            string containerName,
            CancellationToken cancellationToken = default)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            var names = new List<string>();

            await foreach (var blob in container.GetBlobsAsync(cancellationToken: cancellationToken))
            {
                names.Add(blob.Name);
            }

            return names;
        }

        public async Task<bool> HasFileAsync(
            string containerName,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            var blob = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(fileName);
            return await blob.ExistsAsync(cancellationToken);
        }
    }
}
