using Application.Abstractions.Services;
using Application.Common.Contracts;
using Application.Common.Models;
using Application.Results;
using Domain.Enums;
using MediatR;

namespace Application.Features.JobAdvertisements.Commands
{
    public partial class CreateJobAdvertisementCommand : IRequest<CreateJobAdvertisementCommand.Response>
    {
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
            public IDataResult<CreatedResponse> Result { get; init; } = null!;
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
    public partial class GetAllJobAdvertisementQuery : IRequest<GetAllJobAdvertisementQuery.Response>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = PageRequest.DefaultPageSize;
        public Guid? EmployerId { get; set; }
        public bool? IsActive { get; set; }

        public string? Skill { get; set; }

        public string? City { get; set; }

        public string? Search { get; set; }

        public bool OrderByHighestSalary { get; set; }

        public sealed class Response
        {
            public IDataResult<PagedResult<JobAdvertisementResponse>> Result { get; init; } = null!;
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
                        new JobAdvertisementFilter
                        {
                            EmployerId = request.EmployerId,
                            IsActive = request.IsActive,
                            Skill = request.Skill,
                            City = request.City,
                            Search = request.Search,
                            OrderByHighestSalary = request.OrderByHighestSalary
                        },
                        cancellationToken)
                };
        }
    }

    public partial class GetByIdJobAdvertisementQuery : IRequest<GetByIdJobAdvertisementQuery.Response>
    {
        public Guid Id { get; set; }

        public sealed class Response
        {
            public IDataResult<JobAdvertisementResponse> Result { get; init; } = null!;
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
