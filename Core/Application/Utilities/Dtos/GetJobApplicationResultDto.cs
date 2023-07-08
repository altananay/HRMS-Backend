using Domain.Common;

namespace Application.Utilities.Dtos
{
    public class GetJobApplicationResultDto : IDto
    {
        public string Id { get; set; }
        public string Result { get; set; }
    }
}