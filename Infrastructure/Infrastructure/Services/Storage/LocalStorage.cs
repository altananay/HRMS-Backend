using Application.Abstractions.Storage;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Storage
{
    public sealed class LocalStorage : IStorage
    {
        private const string DefaultRelativeRoot = "App_Data/uploads";

        private readonly string _rootPath;
        private readonly ILogger<LocalStorage> _logger;

        public LocalStorage(IConfiguration configuration, IHostEnvironment environment, ILogger<LocalStorage> logger)
        {
            _logger = logger;

            var configured = configuration["Storage:Local:RootPath"];

            var contentRoot = string.IsNullOrWhiteSpace(environment.ContentRootPath)
                ? AppContext.BaseDirectory
                : environment.ContentRootPath;

            _rootPath = Path.GetFullPath(string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(contentRoot, DefaultRelativeRoot)
                : Path.IsPathRooted(configured) ? configured : Path.Combine(contentRoot, configured));

            GuardAgainstPubliclyServedRoot(contentRoot);
        }

        private void GuardAgainstPubliclyServedRoot(string contentRoot)
        {
            var webRoot = Path.GetFullPath(Path.Combine(contentRoot, "wwwroot"));

            var isUnderWebRoot = _rootPath.Equals(webRoot, StringComparison.OrdinalIgnoreCase)
                || _rootPath.StartsWith(webRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

            if (isUnderWebRoot)
            {
                throw new InvalidOperationException(
                    $"Storage:Local:RootPath resolves to '{_rootPath}', which is inside the web root " +
                    $"('{webRoot}') and therefore served publicly by UseStaticFiles(). Uploaded CVs are " +
                    "personal data and must not be reachable without authorization. Point it at a " +
                    $"directory outside wwwroot, e.g. '{DefaultRelativeRoot}'.");
            }

            _logger.LogInformation("Local storage root: {RootPath}", _rootPath);
        }

        public async Task<IReadOnlyList<StoredFile>> UploadAsync(
            string containerName,
            IReadOnlyList<FileUploadRequest> files,
            CancellationToken cancellationToken = default)
        {
            var directory = Path.Combine(_rootPath, containerName);
            Directory.CreateDirectory(directory);

            var stored = new List<StoredFile>(files.Count);

            foreach (var file in files)
            {
                var safeName = BuildUniqueFileName(directory, file.FileName);
                var fullPath = Path.Combine(directory, safeName);

                await using (var target = File.Create(fullPath))
                {
                    await file.Content.CopyToAsync(target, cancellationToken);
                }

                stored.Add(new StoredFile(
                    safeName,
                    Path.Combine(containerName, safeName).Replace('\\', '/'),
                    file.ContentType,
                    file.Length));
            }

            _logger.LogInformation("Stored {Count} file(s) in local container {Container}", stored.Count, containerName);

            return stored;
        }

        public Task<Stream?> OpenReadAsync(
            string containerName,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            var fullPath = ResolveWithinRoot(containerName, fileName);

            Stream? stream = File.Exists(fullPath)
                ? new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true)
                : null;

            return Task.FromResult(stream);
        }

        private string ResolveWithinRoot(string containerName, string fileName)
        {
            var relative = StorageKey.StripContainer(containerName, fileName);

            var candidate = Path.GetFullPath(Path.Combine(_rootPath, containerName, relative));

            if (!candidate.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                && !candidate.Equals(_rootPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException($"Resolved path '{candidate}' escapes the storage root.");
            }

            return candidate;
        }

        public Task DeleteAsync(string containerName, string fileName, CancellationToken cancellationToken = default)
        {
            var fullPath = ResolveWithinRoot(containerName, fileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<string>> GetFilesAsync(string containerName, CancellationToken cancellationToken = default)
        {
            var directory = Path.Combine(_rootPath, containerName);

            IReadOnlyList<string> files = Directory.Exists(directory)
                ? Directory.GetFiles(directory).Select(Path.GetFileName).OfType<string>().ToList()
                : [];

            return Task.FromResult(files);
        }

        public Task<bool> HasFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default)
            => Task.FromResult(File.Exists(ResolveWithinRoot(containerName, fileName)));

        private static string BuildUniqueFileName(string directory, string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var baseName = Path.GetFileNameWithoutExtension(originalFileName);

            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                baseName = baseName.Replace(invalid, '_');
            }

            if (string.IsNullOrWhiteSpace(baseName))
            {
                baseName = "file";
            }

            var candidate = $"{baseName}{extension}";
            var attempt = 1;

            while (File.Exists(Path.Combine(directory, candidate)))
            {
                candidate = $"{baseName}-{attempt++}{extension}";
            }

            return candidate;
        }
    }

    public sealed class StorageService : IStorageService
    {
        private readonly IStorage _storage;

        public StorageService(IStorage storage, IConfiguration configuration)
        {
            _storage = storage;
            Provider = Enum.TryParse<StorageProvider>(configuration["Storage:Provider"], ignoreCase: true, out var provider)
                ? provider
                : StorageProvider.Local;
        }

        public StorageProvider Provider { get; }

        public Task<IReadOnlyList<StoredFile>> UploadAsync(
            string containerName, IReadOnlyList<FileUploadRequest> files, CancellationToken cancellationToken = default)
            => _storage.UploadAsync(containerName, files, cancellationToken);

        public Task DeleteAsync(string containerName, string fileName, CancellationToken cancellationToken = default)
            => _storage.DeleteAsync(containerName, fileName, cancellationToken);

        public Task<IReadOnlyList<string>> GetFilesAsync(string containerName, CancellationToken cancellationToken = default)
            => _storage.GetFilesAsync(containerName, cancellationToken);

        public Task<bool> HasFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default)
            => _storage.HasFileAsync(containerName, fileName, cancellationToken);

        public Task<Stream?> OpenReadAsync(string containerName, string fileName, CancellationToken cancellationToken = default)
            => _storage.OpenReadAsync(containerName, fileName, cancellationToken);
    }
}
