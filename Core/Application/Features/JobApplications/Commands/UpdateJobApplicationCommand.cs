using Application.Abstractions;
using Application.Results;
using MediatR;
using static Application.Features.JobApplications.Commands.UpdateJobApplicationCommand;

namespace Application.Features.JobApplications.Commands
{
    public partial class UpdateJobApplicationCommand : IRequest<UpdateJobApplicationCommandResponse>
    {
        public string? JobApplicationId { get; set; }
        public string EmployerDescription { get; set; }
        public string Result { get; set; }

        public class UpdateJobApplicationCommandResponse
        {
            public IResult Result;
        }

        public class UpdateJobApplicationCommandHandler : IRequestHandler<UpdateJobApplicationCommand, UpdateJobApplicationCommandResponse>
        {
            private readonly IJobApplicationService _jobApplicationService;

            public UpdateJobApplicationCommandHandler(IJobApplicationService jobApplicationService)
            {
                _jobApplicationService = jobApplicationService;
            }

            public async Task<UpdateJobApplicationCommandResponse> Handle(UpdateJobApplicationCommand request, CancellationToken cancellationToken)
            {
                var result = await _jobApplicationService.Update(request);
                return new UpdateJobApplicationCommandResponse { Result = result };
            }
        }
    }
}