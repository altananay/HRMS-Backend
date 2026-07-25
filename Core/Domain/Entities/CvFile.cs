using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// An uploaded CV document.
    /// </summary>
    /// <remarks>
    /// <see cref="CvId"/> is the foreign key this entity never had. Previously CvFile held only
    /// FileName, Path and Storage, and <c>UploadCvFileCommand</c> bulk-inserted rows with no link to
    /// any seeker or CV — so uploaded files were unattributable the moment they were written.
    /// </remarks>
    public class CvFile : BaseEntity
    {
        public Guid CvId { get; set; }

        public Cv Cv { get; set; } = null!;

        public string FileName { get; set; } = null!;

        public string StoragePath { get; set; } = null!;

        public StorageProvider StorageProvider { get; set; }

        public string? ContentType { get; set; }

        public long SizeBytes { get; set; }
    }
}
