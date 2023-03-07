using Domain.Common;

namespace Application.Dtos
{
    public class CreateProjectDto : IDto
    {
        public string ProjectName { get; set; }
        public string Description { get; set; }
    }
}