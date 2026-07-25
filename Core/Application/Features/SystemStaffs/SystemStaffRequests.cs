using Application.Abstractions.Services;
using Application.Common.Dtos;
using Application.Common.Models;
using Application.Results;
using MediatR;

namespace Application.Features.SystemStaffs.Commands
{
    public partial class UpdateSystemStaffCommand : IRequest<UpdateSystemStaffCommand.Response>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;

        /// <remarks>
        /// There is deliberately no <c>Claims</c> property here. The old CreateSystemStaffCommand
        /// exposed one and AutoMapper copied it straight onto the entity, so any caller who reached
        /// this endpoint could grant themselves the admin role. Roles are assigned server-side only.
        /// </remarks>
        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<UpdateSystemStaffCommand, Response>
        {
            private readonly ISystemStaffService _systemStaffService;

            public Handler(ISystemStaffService systemStaffService) => _systemStaffService = systemStaffService;

            public async Task<Response> Handle(UpdateSystemStaffCommand request, CancellationToken cancellationToken)
                => new() { Result = await _systemStaffService.UpdateAsync(request, cancellationToken) };
        }
    }

    public partial class DeleteSystemStaffCommand : IRequest<DeleteSystemStaffCommand.Response>
    {
        public Guid Id { get; set; }

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<DeleteSystemStaffCommand, Response>
        {
            private readonly ISystemStaffService _systemStaffService;

            public Handler(ISystemStaffService systemStaffService) => _systemStaffService = systemStaffService;

            public async Task<Response> Handle(DeleteSystemStaffCommand request, CancellationToken cancellationToken)
                => new() { Result = await _systemStaffService.DeleteAsync(request.Id, cancellationToken) };
        }
    }
}

namespace Application.Features.SystemStaffs.Queries
{
    public partial class GetAllSystemStaffQuery : IRequest<GetAllSystemStaffQuery.Response>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = PageRequest.DefaultPageSize;

        public sealed class Response
        {
            public IDataResult<PagedResult<SystemStaffDto>> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetAllSystemStaffQuery, Response>
        {
            private readonly ISystemStaffService _systemStaffService;

            public Handler(ISystemStaffService systemStaffService) => _systemStaffService = systemStaffService;

            public async Task<Response> Handle(GetAllSystemStaffQuery request, CancellationToken cancellationToken)
                => new()
                {
                    Result = await _systemStaffService.GetPagedAsync(
                        new PageRequest(request.Page, request.PageSize), cancellationToken)
                };
        }
    }

    public partial class GetByIdSystemStaffQuery : IRequest<GetByIdSystemStaffQuery.Response>
    {
        public Guid Id { get; set; }

        public sealed class Response
        {
            public IDataResult<SystemStaffDto> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetByIdSystemStaffQuery, Response>
        {
            private readonly ISystemStaffService _systemStaffService;

            public Handler(ISystemStaffService systemStaffService) => _systemStaffService = systemStaffService;

            public async Task<Response> Handle(GetByIdSystemStaffQuery request, CancellationToken cancellationToken)
                => new() { Result = await _systemStaffService.GetByIdAsync(request.Id, cancellationToken) };
        }
    }
}
