using Domain.Common;

namespace Application.Dtos
{
    public class UpdateProjectDto : IDto
    {
        public string ProjectName { get; set; }
        public string Description { get; set; }
    }
}