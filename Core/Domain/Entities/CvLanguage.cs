using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// A language a seeker speaks. The old type was <c>Language</c> with a <c>Languages</c> property
    /// holding one name and a free-text <c>LanguageLevel</c>; both are now singular and typed.
    /// </summary>
    public class CvLanguage : BaseEntity
    {
        public Guid CvId { get; set; }

        public Cv Cv { get; set; } = null!;

        public string Name { get; set; } = null!;

        public LanguageLevel Level { get; set; }
    }
}
