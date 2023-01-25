using Application.Abstractions;
using Application.Results;
using MediatR;
using static Application.Features.JobSeekers.Commands.DeleteJobSeekerCommand;

namespace Application.Features.JobSeekers.Commands
{
    public partial class DeleteJobSeekerCommand : IRequest<DeleteJobSeekerCommandResponse>
    {
        public string Id { get; set; }

        public class DeleteJobSeekerCommandResponse
        {
            public IResult Result { get; set; }
        }

        public class DeleteJobSeekerCommandHandler : IRequestHandler<DeleteJobSeekerCommand, DeleteJobSeekerCommandResponse>
        {
            private readonly IJobSeekerService _jobSeekerService;

            public DeleteJobSeekerCommandHandler(IJobSeekerService jobSeekerService)
            {
                _jobSeekerService = jobSeekerService;
            }

            public async Task<DeleteJobSeekerCommandResponse> Handle(DeleteJobSeekerCommand request, CancellationToken cancellationToken)
            {
                var result = await _jobSeekerService.Delete(request.Id);
                return new DeleteJobSeekerCommandResponse { Result = result };
            }
        }
    }
}