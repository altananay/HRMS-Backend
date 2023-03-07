using Domain.Common;

namespace Application.Dtos
{
    public class CreateJobExperienceDto : IDto
    {
        public string CompanyName { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string Years { get; set; }
        public string Description { get; set; }
        public string? LeaveWorkYear { get; set; }
    }
}