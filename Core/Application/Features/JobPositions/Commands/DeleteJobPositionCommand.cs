using Application.Abstractions;
using Application.Results;
using MediatR;
using static Application.Features.JobPositions.Commands.DeleteJobPositionCommand;
using static Application.Features.JobPositions.Queries.GetJobPositionQuery;

namespace Application.Features.JobPositions.Commands
{
    public partial class DeleteJobPositionCommand : IRequest<DeleteJobPositionCommandResponse>
    {
        public string Id { get; set; }

        public class DeleteJobPositionCommandResponse
        {
            public IResult Result;
        }

        public class DeleteJobPositionCommandHandler : IRequestHandler<DeleteJobPositionCommand, DeleteJobPositionCommandResponse>
        {
            private readonly IJobPositionService _jobPositionService;

            public DeleteJobPositionCommandHandler(IJobPositionService jobPositionService)
            {
                _jobPositionService = jobPositionService;
            }

            public async Task<DeleteJobPositionCommandResponse> Handle(DeleteJobPositionCommand request, CancellationToken cancellationToken)
            {
                var result = await _jobPositionService.Delete(request.Id);
                if (result.IsSuccess)
                {
                    return new DeleteJobPositionCommandResponse { Result = result };
                }

                return new DeleteJobPositionCommandResponse { Result = result };
            }
        }
    }
}