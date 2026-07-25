using Domain.Common;

namespace Domain.Entities
{
    /// <summary>
    /// A portfolio project on a CV. Named <c>CvProject</c> rather than <c>Project</c> so it does not
    /// collide with MSBuild/EF tooling types named Project.
    /// </summary>
    public class CvProject : BaseEntity
    {
        public Guid CvId { get; set; }

        public Cv Cv { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? Description { get; set; }
    }
}
