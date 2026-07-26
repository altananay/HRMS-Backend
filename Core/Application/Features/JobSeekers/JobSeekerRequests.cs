using Application.Abstractions.Services;
using Application.Common.Dtos;
using Application.Common.Models;
using Application.Results;
using MediatR;

namespace Application.Features.JobSeekers.Commands
{
    public partial class UpdateJobSeekerCommand : IRequest<UpdateJobSeekerCommand.Response>
    {
        public Guid Id { get; set; }

        /// <remarks>
        /// All of these are actually applied. The old manager took the full command and then wrote
        /// only <c>Email</c>, silently discarding every other field the caller sent.
        /// </remarks>
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateOnly? DateOfBirth { get; set; }

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<UpdateJobSeekerCommand, Response>
        {
            private readonly IJobSeekerService _jobSeekerService;

            public Handler(IJobSeekerService jobSeekerService) => _jobSeekerService = jobSeekerService;

            public async Task<Response> Handle(UpdateJobSeekerCommand request, CancellationToken cancellationToken)
                => new() { Result = await _jobSeekerService.UpdateAsync(request, cancellationToken) };
        }
    }

    public partial class DeleteJobSeekerCommand : IRequest<DeleteJobSeekerCommand.Response>
    {
        public Guid Id { get; set; }

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<DeleteJobSeekerCommand, Response>
        {
            private readonly IJobSeekerService _jobSeekerService;

            public Handler(IJobSeekerService jobSeekerService) => _jobSeekerService = jobSeekerService;

            public async Task<Response> Handle(DeleteJobSeekerCommand request, CancellationToken cancellationToken)
                => new() { Result = await _jobSeekerService.DeleteAsync(request.Id, cancellationToken) };
        }
    }
}

namespace Application.Features.JobSeekers.Queries
{
    public partial class GetAllJobSeekerQuery : IRequest<GetAllJobSeekerQuery.Response>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = PageRequest.DefaultPageSize;

        public sealed class Response
        {
            public IDataResult<PagedResult<JobSeekerDto>> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetAllJobSeekerQuery, Response>
        {
            private readonly IJobSeekerService _jobSeekerService;

            public Handler(IJobSeekerService jobSeekerService) => _jobSeekerService = jobSeekerService;

            public async Task<Response> Handle(GetAllJobSeekerQuery request, CancellationToken cancellationToken)
                => new()
                {
                    Result = await _jobSeekerService.GetPagedAsync(
                        new PageRequest(request.Page, request.PageSize), cancellationToken)
                };
        }
    }

    public partial class GetByIdJobSeekerQuery : IRequest<GetByIdJobSeekerQuery.Response>
    {
        public Guid Id { get; set; }

        /// <summary>Set by the controller from the token, never bound from the request.</summary>
        public Guid RequestedBy { get; set; }

        public sealed class Response
        {
            public IDataResult<JobSeekerDto> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetByIdJobSeekerQuery, Response>
        {
            private readonly IJobSeekerService _jobSeekerService;

            public Handler(IJobSeekerService jobSeekerService) => _jobSeekerService = jobSeekerService;

            public async Task<Response> Handle(GetByIdJobSeekerQuery request, CancellationToken cancellationToken)
                => new() { Result = await _jobSeekerService.GetByIdAsync(request.Id, request.RequestedBy, cancellationToken) };
        }
    }

    public partial class GetByEmailJobSeekerQuery : IRequest<GetByEmailJobSeekerQuery.Response>
    {
        public string Email { get; set; } = null!;

        public sealed class Response
        {
            public IDataResult<JobSeekerDto> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetByEmailJobSeekerQuery, Response>
        {
            private readonly IJobSeekerService _jobSeekerService;

            public Handler(IJobSeekerService jobSeekerService) => _jobSeekerService = jobSeekerService;

            public async Task<Response> Handle(GetByEmailJobSeekerQuery request, CancellationToken cancellationToken)
                => new() { Result = await _jobSeekerService.GetByEmailAsync(request.Email, cancellationToken) };
        }
    }
}
