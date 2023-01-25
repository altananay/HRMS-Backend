using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.SystemStaffs.Queries.GetByIdSystemStaffQuery;

namespace Application.Features.SystemStaffs.Queries
{
    public partial class GetByIdSystemStaffQuery : IRequest<GetByIdSystemStaffQueryResponse>
    {
        public string Id { get; set; }

        public class GetByIdSystemStaffQueryResponse
        {
            public IDataResult<SystemStaff> SystemStaff;
        }

        public class GetByIdSystemStaffQueryHandler : IRequestHandler<GetByIdSystemStaffQuery, GetByIdSystemStaffQueryResponse>
        {
            private readonly ISystemStaffService _systemStaffService;

            public GetByIdSystemStaffQueryHandler(ISystemStaffService systemStaffService)
            {
                _systemStaffService = systemStaffService;
            }


            public async Task<GetByIdSystemStaffQueryResponse> Handle(GetByIdSystemStaffQuery request, CancellationToken cancellationToken)
            {
                var result = _systemStaffService.GetById(request.Id);
                return new GetByIdSystemStaffQueryResponse { SystemStaff = result };
            }
        }
    }
}