using Application.Abstractions.Services;
using Application.Common.Contracts;
using Application.Common.Models;
using Application.Results;
using MediatR;

namespace Application.Features.Employers.Commands
{
    public partial class UpdateEmployerCommand : IRequest<UpdateEmployerCommand.Response>
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? CompanyPhone { get; set; }
        public string? WebSite { get; set; }
        public int? NumberOfEmployees { get; set; }
        public string? Description { get; set; }
        public string[] Sectors { get; set; } = [];
        public List<DepartmentRequest> Departments { get; set; } = [];

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<UpdateEmployerCommand, Response>
        {
            private readonly IEmployerService _employerService;

            public Handler(IEmployerService employerService) => _employerService = employerService;

            public async Task<Response> Handle(UpdateEmployerCommand request, CancellationToken cancellationToken)
                => new() { Result = await _employerService.UpdateAsync(request, cancellationToken) };
        }
    }

    public partial class DeleteEmployerCommand : IRequest<DeleteEmployerCommand.Response>
    {
        public Guid Id { get; set; }

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<DeleteEmployerCommand, Response>
        {
            private readonly IEmployerService _employerService;

            public Handler(IEmployerService employerService) => _employerService = employerService;

            public async Task<Response> Handle(DeleteEmployerCommand request, CancellationToken cancellationToken)
                => new() { Result = await _employerService.DeleteAsync(request.Id, cancellationToken) };
        }
    }
}

namespace Application.Features.Employers.Queries
{
    /// <summary>The anonymous company directory.</summary>
    public partial class GetPublicEmployerQuery : IRequest<GetPublicEmployerQuery.Response>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = PageRequest.DefaultPageSize;

        public sealed class Response
        {
            public IDataResult<PagedResult<EmployerSummaryResponse>> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetPublicEmployerQuery, Response>
        {
            private readonly IEmployerService _employerService;

            public Handler(IEmployerService employerService) => _employerService = employerService;

            public async Task<Response> Handle(GetPublicEmployerQuery request, CancellationToken cancellationToken)
                => new()
                {
                    Result = await _employerService.GetPublicPagedAsync(
                        new PageRequest(request.Page, request.PageSize), cancellationToken)
                };
        }
    }

    public partial class GetAllEmployerQuery : IRequest<GetAllEmployerQuery.Response>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = PageRequest.DefaultPageSize;

        public bool OrderByNumberOfEmployees { get; set; }

        public sealed class Response
        {
            public IDataResult<PagedResult<EmployerResponse>> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetAllEmployerQuery, Response>
        {
            private readonly IEmployerService _employerService;

            public Handler(IEmployerService employerService) => _employerService = employerService;

            public async Task<Response> Handle(GetAllEmployerQuery request, CancellationToken cancellationToken)
                => new()
                {
                    Result = await _employerService.GetPagedAsync(
                        new PageRequest(request.Page, request.PageSize),
                        request.OrderByNumberOfEmployees,
                        cancellationToken)
                };
        }
    }

    public partial class GetByIdEmployerQuery : IRequest<GetByIdEmployerQuery.Response>
    {
        public Guid Id { get; set; }

        public sealed class Response
        {
            public IDataResult<EmployerDetailResponse> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetByIdEmployerQuery, Response>
        {
            private readonly IEmployerService _employerService;

            public Handler(IEmployerService employerService) => _employerService = employerService;

            public async Task<Response> Handle(GetByIdEmployerQuery request, CancellationToken cancellationToken)
                => new() { Result = await _employerService.GetByIdAsync(request.Id, cancellationToken) };
        }
    }

    public partial class GetByEmailEmployerQuery : IRequest<GetByEmailEmployerQuery.Response>
    {
        public string Email { get; set; } = null!;

        public sealed class Response
        {
            public IDataResult<EmployerResponse> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetByEmailEmployerQuery, Response>
        {
            private readonly IEmployerService _employerService;

            public Handler(IEmployerService employerService) => _employerService = employerService;

            public async Task<Response> Handle(GetByEmailEmployerQuery request, CancellationToken cancellationToken)
                => new() { Result = await _employerService.GetByEmailAsync(request.Email, cancellationToken) };
        }
    }
}
