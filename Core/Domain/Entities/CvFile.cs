using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
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
