using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobPositions.Commands.UpdateJobPositionCommand;

namespace Application.Features.JobPositions.Commands
{
    public class UpdateJobPositionCommand : IRequest<UpdateJobPositionCommandResponse>
    {
        public string Id { get; set; }
        public string PositionName { get; set; }

        public class UpdateJobPositionCommandResponse
        {
            public IResult Result;
        }

        public class UpdateJobPositionCommandHandler : IRequestHandler<UpdateJobPositionCommand, UpdateJobPositionCommandResponse>
        {
            private readonly IJobPositionService _jobPositionService;

            public UpdateJobPositionCommandHandler(IJobPositionService jobPositionService)
            {
                _jobPositionService = jobPositionService;
            }

            public async Task<UpdateJobPositionCommandResponse> Handle(UpdateJobPositionCommand request, CancellationToken cancellationToken)
            {
                JobPosition jobPosition = new();
                var result = _jobPositionService.GetById(request.Id);
                jobPosition.Id = request.Id;
                jobPosition.CreatedAt = result.Data.CreatedAt;
                jobPosition.UpdatedAt = DateTime.UtcNow;
                jobPosition.PositionName = request.PositionName;

                var result2 = await _jobPositionService.Update(jobPosition);
                if (result2.IsSuccess)
                {
                    return new UpdateJobPositionCommandResponse { Result = result2 };
                }
                return new UpdateJobPositionCommandResponse { Result = result2 };
            }
        }

    }
}