using Application.Abstractions.Storage;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
        /// <summary>Outside wwwroot, deliberately — see <see cref="GuardAgainstPubliclyServedRoot"/>.</summary>
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

        /// <summary>
        /// Refuses to start if uploads would land inside the statically-served web root.
        /// </summary>
        /// <remarks>
        /// This is not hypothetical. The configured default was <c>wwwroot/uploads</c> while
        /// <c>Program.cs</c> calls <c>UseStaticFiles()</c>, and in a published application the
        /// content root and the base directory are the same folder — so every uploaded CV would have
        /// been anonymously downloadable by anyone who could guess its URL. That is the same class of
        /// mistake as the <c>PublicAccessType.BlobContainer</c> the Azure adapter used to set.
        ///
        /// A comment warning against it would not have prevented the next person reinstating it, so
        /// this fails loudly at startup instead. CVs are personal data and are served only through
        /// the authorized download endpoint.
        /// </remarks>
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
