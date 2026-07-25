using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Abstractions.Storage;
using Application.Common.Dtos;
using Application.Common.Exceptions;
using Application.Features.Cvs.Commands;
using Application.Mapping;
using Application.Results;
using Application.Rules;
using Application.Utilities.Constants;
using Domain.Entities;

namespace Application.Services
{
    public sealed class CvFileManager : ICvFileService
    {
        /// <summary>Container/prefix under which CV attachments are stored.</summary>
        private const string ContainerName = "cv-files";

        private const long MaxFileSizeBytes = 5 * 1024 * 1024;
        private const int MaxFilesPerCv = 5;

        /// <summary>
        /// Accepted document types, and the extensions each one may claim.
        /// </summary>
        /// <remarks>
        /// Both halves are checked. A content type alone is caller-supplied and trivially spoofed;
        /// an extension alone says nothing about the payload. Requiring them to agree at least stops
        /// <c>payload.exe</c> arriving labelled <c>application/pdf</c>.
        /// </remarks>
        private static readonly Dictionary<string, string[]> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["application/pdf"] = [".pdf"],
            ["application/msword"] = [".doc"],
            ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = [".docx"]
        };

        private readonly ICvRepository _cvs;
        private readonly ICvFileRepository _cvFiles;
        private readonly IJobApplicationRepository _applications;
        private readonly IStorageService _storage;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly BusinessRules _rules;

        public CvFileManager(
            ICvRepository cvs,
            ICvFileRepository cvFiles,
            IJobApplicationRepository applications,
            IStorageService storage,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            BusinessRules rules)
        {
            _cvs = cvs;
            _cvFiles = cvFiles;
            _applications = applications;
            _storage = storage;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _rules = rules;
        }

        public async Task<IDataResult<IReadOnlyList<CvFileDto>>> UploadAsync(
            UploadCvFileCommand command,
            CancellationToken cancellationToken = default)
        {
            if (command.Files.Count == 0)
            {
                throw new ConflictException("Yüklenecek dosya bulunamadı.");
            }

            // The CV is resolved from the caller's own id, so a file always lands on a real CV owned
            // by whoever uploaded it.
            var cv = await _rules.EnsureCvExistsForJobSeekerAsync(command.JobSeekerId, cancellationToken);

            if (cv.Files.Count + command.Files.Count > MaxFilesPerCv)
            {
                throw new ConflictException($"Bir CV'ye en fazla {MaxFilesPerCv} dosya eklenebilir.");
            }

            foreach (var file in command.Files)
            {
                Validate(file);
            }

            var stored = await _storage.UploadAsync(ContainerName, command.Files, cancellationToken);

            var entities = stored.Select(item => new CvFile
            {
                CvId = cv.Id,
                FileName = item.FileName,
                StoragePath = item.StoragePath,
                StorageProvider = _storage.Provider,
                ContentType = item.ContentType,
                SizeBytes = item.SizeBytes
            }).ToList();

            _cvFiles.AddRange(entities);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessDataResult<IReadOnlyList<CvFileDto>>(
                entities.Select(DomainMapper.ToDto).ToList(), Messages.Cv.Updated);
        }

        public async Task<CvFileDownload> DownloadAsync(
            Guid fileId,
            Guid requestedBy,
            CancellationToken cancellationToken = default)
        {
            var file = await _cvFiles.GetByIdWithCvAsync(fileId, cancellationToken)
                ?? throw new NotFoundException(Messages.Cv.NotFound);

            await EnsureCanReadAsync(file, requestedBy, cancellationToken);

            var content = await _storage.OpenReadAsync(ContainerName, file.StoragePath, cancellationToken)
                ?? throw new NotFoundException(Messages.Cv.NotFound);

            return new CvFileDownload(content, file.FileName, file.ContentType ?? "application/octet-stream");
        }

        public async Task<IResult> DeleteAsync(
            Guid fileId,
            Guid requestedBy,
            CancellationToken cancellationToken = default)
        {
            var file = await _cvFiles.GetByIdWithCvAsync(fileId, cancellationToken)
                ?? throw new NotFoundException(Messages.Cv.NotFound);

            // Deleting is stricter than reading: only the owner or an admin, never an employer who
            // merely received an application.
            if (file.Cv.JobSeekerId != requestedBy && !_currentUser.IsInRole(Roles.Admin))
            {
                throw new ForbiddenException(Messages.Authentication.AuthorizationDenied);
            }

            await _storage.DeleteAsync(ContainerName, file.StoragePath, cancellationToken);

            _cvFiles.Remove(file);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.Cv.Deleted);
        }

        /// <summary>
        /// Allows the owning seeker, an employer holding an application from them, or an admin.
        /// </summary>
        /// <remarks>
        /// The employer case is the reason this is a database question rather than a role check: an
        /// employer may read a CV precisely because that seeker applied to one of their
        /// advertisements, and not otherwise. A plain <c>[Authorize(Roles = "employer")]</c> would
        /// let any employer read every CV in the system.
        /// </remarks>
        private async Task EnsureCanReadAsync(CvFile file, Guid requestedBy, CancellationToken cancellationToken)
        {
            if (file.Cv.JobSeekerId == requestedBy || _currentUser.IsInRole(Roles.Admin))
            {
                return;
            }

            if (_currentUser.IsInRole(Roles.Employer))
            {
                var hasApplication = await _applications.ExistsForEmployerAndSeekerAsync(
                    requestedBy, file.Cv.JobSeekerId, cancellationToken);

                if (hasApplication)
                {
                    return;
                }
            }

            throw new ForbiddenException(Messages.Authentication.AuthorizationDenied);
        }

        private static void Validate(FileUploadRequest file)
        {
            if (file.Length <= 0)
            {
                throw new ConflictException($"'{file.FileName}' boş.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                throw new ConflictException(
                    $"'{file.FileName}' çok büyük. En fazla {MaxFileSizeBytes / 1024 / 1024} MB yükleyebilirsiniz.");
            }

            if (file.ContentType is null || !AllowedContentTypes.TryGetValue(file.ContentType, out var extensions))
            {
                throw new ConflictException($"'{file.FileName}' desteklenmeyen bir dosya türü. PDF, DOC veya DOCX yükleyin.");
            }

            var extension = Path.GetExtension(file.FileName);

            if (!extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                throw new ConflictException(
                    $"'{file.FileName}' uzantısı içerik türüyle uyuşmuyor.");
            }
        }
    }
}
