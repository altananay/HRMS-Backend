using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class CvLanguage : BaseEntity
    {
        public Guid CvId { get; set; }

        public Cv Cv { get; set; } = null!;

        public string Name { get; set; } = null!;

        public LanguageLevel Level { get; set; }
    }
}
