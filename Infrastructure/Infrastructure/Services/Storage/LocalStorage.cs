using Application.Abstractions.Storage;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Storage
{
    /// <summary>
    /// Writes uploads to the local filesystem. The default in Development and tests.
    /// </summary>
    /// <remarks>
    /// The previous implementation was unusable and unreachable. Its upload method read:
    /// <code>
    /// if (!Directory.Exists(uploadPath)) { Directory.CreateDirectory(uploadPath); }
    /// else { throw new Exception("Dosyalar yüklenirken hata oluştu."); }
    /// </code>
    /// — it threw whenever the target directory already existed, so the second upload always
    /// failed. It also never appeared in any DI registration, so nothing could reach it: storage was
    /// hard-wired to Azure with no configuration switch, which is why the functional tests would
    /// otherwise need a real Azure account.
    /// </remarks>
    public sealed class LocalStorage : IStorage
    {
        private readonly string _rootPath;
        private readonly ILogger<LocalStorage> _logger;

        public LocalStorage(IConfiguration configuration, ILogger<LocalStorage> logger)
        {
            _logger = logger;
            var configured = configuration["Storage:Local:RootPath"];

            _rootPath = string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(AppContext.BaseDirectory, "uploads")
                : Path.IsPathRooted(configured) ? configured : Path.Combine(AppContext.BaseDirectory, configured);
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

        public Task DeleteAsync(string containerName, string fileName, CancellationToken cancellationToken = default)
        {
            var fullPath = Path.Combine(_rootPath, containerName, fileName);

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
            => Task.FromResult(File.Exists(Path.Combine(_rootPath, containerName, fileName)));

        /// <summary>
        /// Produces a collision-free name, preserving the extension.
        /// </summary>
        /// <remarks>
        /// The old helper recursed through a <c>await Task.Run(async () =&gt; ...)</c> wrapper for no
        /// reason and, in AzureStorage, was passed <c>file.Name</c> — the form field name — instead
        /// of <c>file.FileName</c>, so every upload in a request collided on a single name.
        /// </remarks>
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
    }
}
