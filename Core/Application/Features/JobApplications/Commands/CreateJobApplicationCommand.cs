using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobApplications.Commands.CreateJobApplicationCommand;

namespace Application.Features.JobApplications.Commands
{
    public partial class CreateJobApplicationCommand : IRequest<CreateJobApplicationCommandResponse>
    {
        public string? JobAdvertisementId { get; set; }
        public string? JobSeekerId { get; set; }
        public string JobSeekerDescription { get; set; }

        public class CreateJobApplicationCommandResponse
        {
            public IResult Result;
        }

        public class CreateJobApplicationCommandHandler : IRequestHandler<CreateJobApplicationCommand, CreateJobApplicationCommandResponse>
        {
            private readonly IJobApplicationService _jobApplicationService;

            public CreateJobApplicationCommandHandler(IJobApplicationService jobApplicationService)
            {
                _jobApplicationService = jobApplicationService;
            }

            public async Task<CreateJobApplicationCommandResponse> Handle(CreateJobApplicationCommand request, CancellationToken cancellationToken)
            {
                var result = await _jobApplicationService.Add(request);
                return new CreateJobApplicationCommandResponse { Result = result };
            }
        }
    }
}
