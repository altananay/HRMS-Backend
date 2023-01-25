using Application.Abstractions;
using Application.Results;
using MediatR;
using static Application.Features.JobApplications.Commands.DeleteJobApplicationCommand;

namespace Application.Features.JobApplications.Commands
{
    public partial class DeleteJobApplicationCommand : IRequest<DeleteJobApplicationCommandResponse>
    {
        public string Id { get; set; }

        public class DeleteJobApplicationCommandResponse
        {
            public IResult Result;
        }

        public class DeleteJobApplicationCommandHandler : IRequestHandler<DeleteJobApplicationCommand, DeleteJobApplicationCommandResponse>
        {
            private readonly IJobApplicationService _jobApplicationService;

            public DeleteJobApplicationCommandHandler(IJobApplicationService jobApplicationService)
            {
                _jobApplicationService = jobApplicationService;
            }

            public async Task<DeleteJobApplicationCommandResponse> Handle(DeleteJobApplicationCommand request, CancellationToken cancellationToken)
            {
                var result = await _jobApplicationService.Delete(request.Id);
                return new DeleteJobApplicationCommandResponse { Result = result };
            }
        }
    }
}