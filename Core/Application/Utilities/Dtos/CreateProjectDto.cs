using Domain.Common;

namespace Application.Utilities.Dtos
{
    public class CreateProjectDto : IDto
    {
        public string ProjectName { get; set; }
        public string Description { get; set; }
    }
}