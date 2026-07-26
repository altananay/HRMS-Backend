using Application.Abstractions.Services;
using Application.Common.Dtos;
using Application.Common.Models;
using Application.Results;
using MediatR;

namespace Application.Features.JobPositions.Commands
{
    public partial class CreateJobPositionCommand : IRequest<CreateJobPositionCommand.Response>
    {
        public string Name { get; set; } = null!;

        public sealed class Response
        {
            public IDataResult<CreatedDto> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<CreateJobPositionCommand, Response>
        {
            private readonly IJobPositionService _jobPositionService;

            public Handler(IJobPositionService jobPositionService) => _jobPositionService = jobPositionService;

            public async Task<Response> Handle(CreateJobPositionCommand request, CancellationToken cancellationToken)
                => new() { Result = await _jobPositionService.AddAsync(request, cancellationToken) };
        }
    }

    public partial class UpdateJobPositionCommand : IRequest<UpdateJobPositionCommand.Response>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<UpdateJobPositionCommand, Response>
        {
            private readonly IJobPositionService _jobPositionService;

            public Handler(IJobPositionService jobPositionService) => _jobPositionService = jobPositionService;

            public async Task<Response> Handle(UpdateJobPositionCommand request, CancellationToken cancellationToken)
                => new() { Result = await _jobPositionService.UpdateAsync(request, cancellationToken) };
        }
    }

    public partial class DeleteJobPositionCommand : IRequest<DeleteJobPositionCommand.Response>
    {
        public Guid Id { get; set; }

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<DeleteJobPositionCommand, Response>
        {
            private readonly IJobPositionService _jobPositionService;

            public Handler(IJobPositionService jobPositionService) => _jobPositionService = jobPositionService;

            public async Task<Response> Handle(DeleteJobPositionCommand request, CancellationToken cancellationToken)
                => new() { Result = await _jobPositionService.DeleteAsync(request.Id, cancellationToken) };
        }
    }
}

namespace Application.Features.JobPositions.Queries
{
    public partial class GetJobPositionQuery : IRequest<GetJobPositionQuery.Response>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = PageRequest.DefaultPageSize;

        public sealed class Response
        {
            public IDataResult<PagedResult<JobPositionDto>> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetJobPositionQuery, Response>
        {
            private readonly IJobPositionService _jobPositionService;

            public Handler(IJobPositionService jobPositionService) => _jobPositionService = jobPositionService;

            public async Task<Response> Handle(GetJobPositionQuery request, CancellationToken cancellationToken)
                => new()
                {
                    Result = await _jobPositionService.GetPagedAsync(
                        new PageRequest(request.Page, request.PageSize), cancellationToken)
                };
        }
    }

    /// <remarks>
    /// This request used to carry <c>[BsonId]</c> and <c>[BsonRepresentation(BsonType.ObjectId)]</c>
    /// on its Id — MongoDB persistence attributes decorating a MediatR message in the Application
    /// layer, which is about as direct a layering violation as it gets.
    /// </remarks>
    public partial class GetJobPositionByIdQuery : IRequest<GetJobPositionByIdQuery.Response>
    {
        public Guid Id { get; set; }

        public sealed class Response
        {
            public IDataResult<JobPositionDto> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetJobPositionByIdQuery, Response>
        {
            private readonly IJobPositionService _jobPositionService;

            public Handler(IJobPositionService jobPositionService) => _jobPositionService = jobPositionService;

            public async Task<Response> Handle(GetJobPositionByIdQuery request, CancellationToken cancellationToken)
                => new() { Result = await _jobPositionService.GetByIdAsync(request.Id, cancellationToken) };
        }
    }
}
