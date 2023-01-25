using Application.Abstractions;
using Application.Results;
using MediatR;
using static Application.Features.JobSeekers.Commands.UpdateJobSeekerCommand;

namespace Application.Features.JobSeekers.Commands
{
    public partial class UpdateJobSeekerCommand : IRequest<UpdateJobSeekerCommandResponse>
    {
        public string Id { get; set; }
        public string Email { get; set; }

        public class UpdateJobSeekerCommandResponse
        {
            public IResult Result;
        }

        public class UpdateJobSeekerCommandHandler : IRequestHandler<UpdateJobSeekerCommand, UpdateJobSeekerCommandResponse>
        {
            private readonly IJobSeekerService _jobSeekerService;

            public UpdateJobSeekerCommandHandler(IJobSeekerService jobSeekerService)
            {
                _jobSeekerService = jobSeekerService;
            }

            public async Task<UpdateJobSeekerCommandResponse> Handle(UpdateJobSeekerCommand request, CancellationToken cancellationToken)
            {
                var result = await _jobSeekerService.Update(request);
                return new UpdateJobSeekerCommandResponse { Result = result };
            }
        }
    }
}