using Domain.Common;

namespace Application.Dtos
{
    public class JobSeekerForLoginDto : IDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}