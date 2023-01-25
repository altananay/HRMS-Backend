using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.Users.Queries.GetAllUserQuery;

namespace Application.Features.Users.Queries
{
    public partial class GetAllUserQuery : IRequest<GetAllUserQueryResponse>
    {
        public class GetAllUserQueryResponse
        {
            public IDataResult<IQueryable<User>> Users;
        }

        public class GetAllUserQueryHandler : IRequestHandler<GetAllUserQuery, GetAllUserQueryResponse>
        {
            private readonly IUserService _userService;

            public GetAllUserQueryHandler(IUserService userService)
            {
                _userService = userService;
            }

            public async Task<GetAllUserQueryResponse> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
            {
                var result = _userService.GetAll();
                return new GetAllUserQueryResponse { Users = result };
            }
        }
    }
}