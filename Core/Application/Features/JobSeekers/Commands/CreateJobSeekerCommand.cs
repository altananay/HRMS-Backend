using Application.Abstractions;
using Application.Results;
using MediatR;
using static Application.Features.JobSeekers.Commands.CreateJobSeekerCommand;

namespace Application.Features.JobSeekers.Commands
{
    public partial class CreateJobSeekerCommand : IRequest<CreateJobSeekerCommandResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string IdentityNumber { get; set; }

        public class CreateJobSeekerCommandResponse
        {
            public IResult Result { get; set; }
        }

        public class CreateJobSeekerCommandHandler : IRequestHandler<CreateJobSeekerCommand, CreateJobSeekerCommandResponse>
        {
            private readonly IJobSeekerService _jobSeekerService;
            private readonly IAuthService _authService;

            public CreateJobSeekerCommandHandler(IJobSeekerService jobSeekerService, IAuthService authService)
            {
                _jobSeekerService = jobSeekerService;
                _authService = authService;
            }

            public async Task<CreateJobSeekerCommandResponse> Handle(CreateJobSeekerCommand request, CancellationToken cancellationToken)
            {
                var result = _jobSeekerService.NationalityIdExists(request.IdentityNumber);
                if (!result.IsSuccess)
                {
                    return new CreateJobSeekerCommandResponse()
                    {
                        Result = result
                    };
                }

                var result2 = await _authService.Register(request, request.Password);
                if (result2.IsSuccess)
                {
                    return new CreateJobSeekerCommandResponse()
                    {
                        Result = result2
                    };
                }

                return new CreateJobSeekerCommandResponse()
                {
                    Result = result2
                };
            }
        }
    }
}