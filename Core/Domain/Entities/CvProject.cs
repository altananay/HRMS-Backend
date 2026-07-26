using Domain.Common;

namespace Domain.Entities
{
    public class CvProject : BaseEntity
    {
        public Guid CvId { get; set; }

        public Cv Cv { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? Description { get; set; }
    }
}
