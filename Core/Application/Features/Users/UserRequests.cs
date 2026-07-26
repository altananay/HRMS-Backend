using Application.Abstractions.Services;
using Application.Common.Contracts;
using Application.Common.Models;
using Application.Results;
using MediatR;

namespace Application.Features.Users.Queries
{
    public partial class GetAllUserQuery : IRequest<GetAllUserQuery.Response>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = PageRequest.DefaultPageSize;

        public sealed class Response
        {
            public IDataResult<PagedResult<UserSummaryResponse>> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetAllUserQuery, Response>
        {
            private readonly IUserService _userService;

            public Handler(IUserService userService) => _userService = userService;

            public async Task<Response> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
                => new()
                {
                    Result = await _userService.GetPagedAsync(
                        new PageRequest(request.Page, request.PageSize), cancellationToken)
                };
        }
    }
}
