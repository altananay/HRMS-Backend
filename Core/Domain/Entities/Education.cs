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

        /// <summary>
        /// Replaces <c>string[] Years</c>, which stored the range as an untyped array and so could
        /// not be sorted, filtered or validated.
        /// </summary>
        public int? StartYear { get; set; }

        public int? EndYear { get; set; }

        public bool IsGraduated { get; set; }
    }
}
