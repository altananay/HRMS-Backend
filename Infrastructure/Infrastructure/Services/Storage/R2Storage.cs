using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Abstractions.Storage;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Storage
{
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

                    ForcePathStyle = true,

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
                var key = $"{containerName}/{Guid.CreateVersion7()}{Path.GetExtension(file.FileName)}";

                var request = new PutObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = key,
                    InputStream = file.Content,
                    ContentType = file.ContentType,

                    // Both required: R2 does not implement the Streaming SigV4 the SDK defaults to.
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

        private static string BuildKey(string containerName, string fileName)
            => StorageKey.Build(containerName, fileName);
    }
}
