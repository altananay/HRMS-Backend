using Application.Abstractions.Services;
using Application.Common.Contracts;
using Application.Common.Models;
using Application.Results;
using Domain.Enums;
using MediatR;

namespace Application.Features.JobApplications.Commands
{
    public partial class CreateJobApplicationCommand : IRequest<CreateJobApplicationCommand.Response>
    {
        public Guid JobAdvertisementId { get; set; }

        public Guid JobSeekerId { get; set; }

        public string? JobSeekerNote { get; set; }

        public sealed class Response
        {
            public IDataResult<CreatedResponse> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<CreateJobApplicationCommand, Response>
        {
            private readonly IJobApplicationService _service;

            public Handler(IJobApplicationService service) => _service = service;

            public async Task<Response> Handle(CreateJobApplicationCommand request, CancellationToken cancellationToken)
                => new() { Result = await _service.AddAsync(request, cancellationToken) };
        }
    }

    public partial class UpdateJobApplicationCommand : IRequest<UpdateJobApplicationCommand.Response>
    {
        public Guid Id { get; set; }

        public Guid EmployerId { get; set; }

        public JobApplicationStatus Status { get; set; }
        public string? EmployerNote { get; set; }

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<UpdateJobApplicationCommand, Response>
        {
            private readonly IJobApplicationService _service;

            public Handler(IJobApplicationService service) => _service = service;

            public async Task<Response> Handle(UpdateJobApplicationCommand request, CancellationToken cancellationToken)
                => new() { Result = await _service.UpdateAsync(request, cancellationToken) };
        }
    }

    public partial class DeleteJobApplicationCommand : IRequest<DeleteJobApplicationCommand.Response>
    {
        public Guid Id { get; set; }

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<DeleteJobApplicationCommand, Response>
        {
            private readonly IJobApplicationService _service;

            public Handler(IJobApplicationService service) => _service = service;

            public async Task<Response> Handle(DeleteJobApplicationCommand request, CancellationToken cancellationToken)
                => new() { Result = await _service.DeleteAsync(request.Id, cancellationToken) };
        }
    }
}

namespace Application.Features.JobApplications.Queries
{
    public partial class GetAllJobApplicationQuery : IRequest<GetAllJobApplicationQuery.Response>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = PageRequest.DefaultPageSize;
        public Guid? EmployerId { get; set; }
        public Guid? JobSeekerId { get; set; }

        public Guid? JobAdvertisementId { get; set; }

        public JobApplicationStatus? Status { get; set; }

        public sealed class Response
        {
            public IDataResult<PagedResult<JobApplicationResponse>> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetAllJobApplicationQuery, Response>
        {
            private readonly IJobApplicationService _service;

            public Handler(IJobApplicationService service) => _service = service;

            public async Task<Response> Handle(GetAllJobApplicationQuery request, CancellationToken cancellationToken)
                => new()
                {
                    Result = await _service.GetPagedAsync(
                        new PageRequest(request.Page, request.PageSize),
                        new JobApplicationFilter
                        {
                            EmployerId = request.EmployerId,
                            JobSeekerId = request.JobSeekerId,
                            JobAdvertisementId = request.JobAdvertisementId,
                            Status = request.Status
                        },
                        cancellationToken)
                };
        }
    }

    public partial class GetByIdJobApplicationQuery : IRequest<GetByIdJobApplicationQuery.Response>
    {
        public Guid Id { get; set; }

        public Guid RequestedBy { get; set; }

        public sealed class Response
        {
            public IDataResult<JobApplicationResponse> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetByIdJobApplicationQuery, Response>
        {
            private readonly IJobApplicationService _service;

            public Handler(IJobApplicationService service) => _service = service;

            public async Task<Response> Handle(GetByIdJobApplicationQuery request, CancellationToken cancellationToken)
                => new() { Result = await _service.GetByIdAsync(request.Id, request.RequestedBy, cancellationToken) };
        }
    }
}
