using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
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
                
                var jobPositionExists = _jobPositionService.JobPositionExists(request.PositionName);
                if (!jobPositionExists.IsSuccess)
                {
                    return new CreateJobPositionCommandResponse
                    {
                        Result = jobPositionExists
                    };
                }

                var position = new JobPosition
                {
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    PositionName = request.PositionName
                };
                var result = await _jobPositionService.Add(position);

                if (result.IsSuccess)
                {
                    return new CreateJobPositionCommandResponse
                    {
                        Result = result
                    };
                }

                return new CreateJobPositionCommandResponse
                {
                    Result = result
                };
            }
        }
    }
}
