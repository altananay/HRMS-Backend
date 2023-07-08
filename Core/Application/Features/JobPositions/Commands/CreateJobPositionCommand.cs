using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobPositions.Commands.CreateJobPositionCommand;

namespace Application.Features.JobPositions.Commands
{
    public partial class CreateJobPositionCommand : IRequest<CreateJobPositionCommandResponse>
    {
        public string PositionName { get; set; }

        public class CreateJobPositionCommandResponse
        {
            public IResult Result;
        }

        public class CreateJobPositionCommandHandler : IRequestHandler<CreateJobPositionCommand, CreateJobPositionCommandResponse>
        {
            private readonly IJobPositionService _jobPositionService;

            public CreateJobPositionCommandHandler(IJobPositionService jobPositionService)
            {
                _jobPositionService = jobPositionService;
            }

            public async Task<CreateJobPositionCommandResponse> Handle(CreateJobPositionCommand request, CancellationToken cancellationToken)
            {
                
                var position = new JobPosition
                {
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    PositionName = request.PositionName
                };
                var result = await _jobPositionService.Add(position);

                return new CreateJobPositionCommandResponse
                {
                    Result = result
                };
            }
        }
    }
}