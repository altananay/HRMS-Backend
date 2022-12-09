using Domain.Common;

namespace Domain.Entities
{
    public class CvFile : BaseEntity
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public string Storage { get; set; }
    }
}