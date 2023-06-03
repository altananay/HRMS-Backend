using Domain.Common;

namespace Application.Dtos
{
    public class GetJobApplicationResultDto : IDto
    {
        public string Id { get; set; }
        public string Result { get; set; }
    }
}