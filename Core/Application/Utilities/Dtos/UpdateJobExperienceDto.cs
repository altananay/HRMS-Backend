using Domain.Common;

namespace Application.Utilities.Dtos
{
    public class UpdateJobExperienceDto : IDto
    {
        public string CompanyName { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string Years { get; set; }
        public string Description { get; set; }
        public string? LeaveWorkYear { get; set; }
    }
}