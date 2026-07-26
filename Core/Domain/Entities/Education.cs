using Domain.Common;

namespace Domain.Entities
{
    public class Education : BaseEntity
    {
        public Guid CvId { get; set; }

        public Cv Cv { get; set; } = null!;

        public string School { get; set; } = null!;

        public string Major { get; set; } = null!;

        public string? Grade { get; set; }

        public int? StartYear { get; set; }

        public int? EndYear { get; set; }

        public bool IsGraduated { get; set; }
    }
}
