using Domain.Common;

namespace Application.Utilities.Dtos
{
    public class UpdateProjectDto : IDto
    {
        public string ProjectName { get; set; }
        public string Description { get; set; }
    }
}