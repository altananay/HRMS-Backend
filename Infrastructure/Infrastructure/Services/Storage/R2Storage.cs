using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Abstractions.Storage;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Storage
{
    /// <summary>
    /// Object storage on Cloudflare R2, via the S3-compatible API.
    /// </summary>
    /// <remarks>
    /// R2 was chosen over AWS S3 and Azure Blob on free-tier terms: 10 GB and unlimited egress, with
    /// no expiry. AWS accounts opened after July 2025 close automatically once the six-month plan
    /// ends, taking the data with them 90 days later — unacceptable for a long-lived project.
    ///
    /// Because this speaks S3, pointing <see cref="R2Options.ServiceUrl"/> elsewhere makes it work
    /// against AWS S3, MinIO or DigitalOcean Spaces unchanged.
    ///
    /// Buckets are never made public and no presigned URLs are issued: downloads go through the
    /// authorized proxy endpoint, so a CV is never reachable without a valid token.
    /// </remarks>
    public sealed class R2Storage : IStorage
    {
        private readonly IAmazonS3 _client;
        private readonly R2Options _options;

        public R2Storage(IOptions<R2Options> options)
        {
            _options = options.Value;

            _client = new AmazonS3Client(
                new BasicAWSCredentials(_options.AccessKeyId, _options.SecretAccessKey),
                new AmazonS3Config
                {
                    ServiceURL = _options.ResolveServiceUrl(),

                    // R2 exposes one bucket per path segment on a single host rather than as
                    // virtual-hosted subdomains.
                    ForcePathStyle = true,

                    // R2 ignores the region but the SDK insists on one being set.
                    AuthenticationRegion = "auto"
                });
        }

        public async Task<IReadOnlyList<StoredFile>> UploadAsync(
            string containerName,
            IReadOnlyList<FileUploadRequest> files,
            CancellationToken cancellationToken = default)
        {
            var stored = new List<StoredFile>(files.Count);

            foreach (var file in files)
            {
                // Stored under an opaque generated key. The original name is kept in the database
                // and re-attached on download, so a caller cannot probe for another CV by guessing
                // file names, and two people uploading "cv.pdf" cannot collide.
                var key = $"{containerName}/{Guid.CreateVersion7()}{Path.GetExtension(file.FileName)}";

                var request = new PutObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = key,
                    InputStream = file.Content,
                    ContentType = file.ContentType,

                    // Both flags are mandatory for R2 and are the single most common reason S3 code
                    // fails against it. R2 does not implement the Streaming SigV4 payload signing
                    // that AWSSDK.S3 uses by default, and recent SDK versions also send a CRC32
                    // integrity header R2 rejects outright with
                    // "Header 'x-amz-checksum-algorithm' with value 'CRC32' not implemented".
                    DisablePayloadSigning = true,
                    DisableDefaultChecksumValidation = true
                };

                await _client.PutObjectAsync(request, cancellationToken);

                stored.Add(new StoredFile(file.FileName, key, file.ContentType, file.Length));
            }

            return stored;
        }

        public async Task<Stream?> OpenReadAsync(
            string containerName,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _client.GetObjectAsync(
                    _options.BucketName, BuildKey(containerName, fileName), cancellationToken);

                return response.ResponseStream;
            }
            catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // A missing object is an expected outcome, not a fault — the caller turns it into 404.
                return null;
            }
        }

        public Task DeleteAsync(string containerName, string fileName, CancellationToken cancellationToken = default)
            => _client.DeleteObjectAsync(_options.BucketName, BuildKey(containerName, fileName), cancellationToken);

        public async Task<IReadOnlyList<string>> GetFilesAsync(
            string containerName,
            CancellationToken cancellationToken = default)
        {
            var response = await _client.ListObjectsV2Async(
                new ListObjectsV2Request { BucketName = _options.BucketName, Prefix = containerName },
                cancellationToken);

            return response.S3Objects.Select(item => item.Key).ToList();
        }

        public async Task<bool> HasFileAsync(
            string containerName,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _client.GetObjectMetadataAsync(
                    _options.BucketName, BuildKey(containerName, fileName), cancellationToken);

                return true;
            }
            catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        /// <summary>Shared with LocalStorage so the two cannot disagree about key shape.</summary>
        private static string BuildKey(string containerName, string fileName)
            => StorageKey.Build(containerName, fileName);
    }
}
