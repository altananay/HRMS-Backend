using Application.Abstractions.Services;
using Application.Common.Dtos;
using Application.Common.Models;
using Application.Results;
using Domain.Enums;
using MediatR;

namespace Application.Features.JobAdvertisements.Commands
{
    public partial class CreateJobAdvertisementCommand : IRequest<CreateJobAdvertisementCommand.Response>
    {
        /// <summary>
        /// Set by the controller from the authenticated employer's token, never bound from the body.
        /// </summary>
        /// <remarks>
        /// This was previously a client-supplied field, so any caller could publish an advertisement
        /// as any employer — an insecure direct object reference across the whole write surface.
        /// </remarks>
        public Guid EmployerId { get; set; }

        public string Title { get; set; } = null!;
        public string JobPositionName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Experience { get; set; }
        public string? City { get; set; }
        public string[] Skills { get; set; } = [];
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string? Currency { get; set; }
        public int OpenPositions { get; set; } = 1;
        public JobType JobType { get; set; }
        public DateOnly Deadline { get; set; }

        public sealed class Response
        {
            public IDataResult<CreatedDto> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<CreateJobAdvertisementCommand, Response>
        {
            private readonly IJobAdvertisementService _service;

            public Handler(IJobAdvertisementService service) => _service = service;

            public async Task<Response> Handle(CreateJobAdvertisementCommand request, CancellationToken cancellationToken)
                => new() { Result = await _service.AddAsync(request, cancellationToken) };
        }
    }

    public partial class UpdateJobAdvertisementCommand : IRequest<UpdateJobAdvertisementCommand.Response>
    {
        public Guid Id { get; set; }

        /// <summary>Taken from the token; used to verify the caller owns this advertisement.</summary>
        public Guid EmployerId { get; set; }

        public string Title { get; set; } = null!;
        public string JobPositionName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Experience { get; set; }
        public string? City { get; set; }
        public string[] Skills { get; set; } = [];
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string? Currency { get; set; }
        public int OpenPositions { get; set; } = 1;
        public JobType JobType { get; set; }
        public DateOnly Deadline { get; set; }

        /// <remarks>
        /// Present and honoured. The old command had no such field and the manager never restored
        /// it, so every edit silently deactivated the advertisement.
        /// </remarks>
        public bool IsActive { get; set; } = true;

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<UpdateJobAdvertisementCommand, Response>
        {
            private readonly IJobAdvertisementService _service;

            public Handler(IJobAdvertisementService service) => _service = service;

            public async Task<Response> Handle(UpdateJobAdvertisementCommand request, CancellationToken cancellationToken)
                => new() { Result = await _service.UpdateAsync(request, cancellationToken) };
        }
    }

    public partial class DeleteJobAdvertisementCommand : IRequest<DeleteJobAdvertisementCommand.Response>
    {
        public Guid Id { get; set; }

        /// <summary>Set by the controller from the token, never bound from the request.</summary>
        public Guid EmployerId { get; set; }

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<DeleteJobAdvertisementCommand, Response>
        {
            private readonly IJobAdvertisementService _service;

            public Handler(IJobAdvertisementService service) => _service = service;

            public async Task<Response> Handle(DeleteJobAdvertisementCommand request, CancellationToken cancellationToken)
                => new() { Result = await _service.DeleteAsync(request.Id, request.EmployerId, cancellationToken) };
        }
    }
}

namespace Application.Features.JobAdvertisements.Queries
{
    /// <summary>
    /// One parameterised query replacing six near-identical ones.
    /// </summary>
    /// <remarks>
    /// The module previously had GetAll, GetAllByStatus, GetAllOrderByHighestSalary,
    /// GetByEmployerId, GetByEmployerIdWithStatus and GetById — six request types, six handlers and
    /// six manager methods differing only in a filter or a sort. Two of them also passed an employer
    /// id to <c>JobAdvertisementExists</c>, which checks advertisement ids.
    /// </remarks>
    public partial class GetAllJobAdvertisementQuery : IRequest<GetAllJobAdvertisementQuery.Response>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = PageRequest.DefaultPageSize;
        public Guid? EmployerId { get; set; }
        public bool? IsActive { get; set; }
        public bool OrderByHighestSalary { get; set; }

        public sealed class Response
        {
            public IDataResult<PagedResult<JobAdvertisementDto>> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetAllJobAdvertisementQuery, Response>
        {
            private readonly IJobAdvertisementService _service;

            public Handler(IJobAdvertisementService service) => _service = service;

            public async Task<Response> Handle(GetAllJobAdvertisementQuery request, CancellationToken cancellationToken)
                => new()
                {
                    Result = await _service.GetPagedAsync(
                        new PageRequest(request.Page, request.PageSize),
                        request.EmployerId,
                        request.IsActive,
                        request.OrderByHighestSalary,
                        cancellationToken)
                };
        }
    }

    public partial class GetByIdJobAdvertisementQuery : IRequest<GetByIdJobAdvertisementQuery.Response>
    {
        public Guid Id { get; set; }

        public sealed class Response
        {
            public IDataResult<JobAdvertisementDto> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetByIdJobAdvertisementQuery, Response>
        {
            private readonly IJobAdvertisementService _service;

            public Handler(IJobAdvertisementService service) => _service = service;

            public async Task<Response> Handle(GetByIdJobAdvertisementQuery request, CancellationToken cancellationToken)
                => new() { Result = await _service.GetByIdAsync(request.Id, cancellationToken) };
        }
    }
}
