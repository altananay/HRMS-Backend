using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.SystemStaffs.Queries.GetAllSystemStaffQuery;

namespace Application.Features.SystemStaffs.Queries
{
    public partial class GetAllSystemStaffQuery : IRequest<GetAllSystemStaffQueryResponse>
    {
        public class GetAllSystemStaffQueryResponse
        {
            public IDataResult<IQueryable<SystemStaff>> SystemStaffs;
        }

        public class GetAllSystemStaffQueryHandler : IRequestHandler<GetAllSystemStaffQuery, GetAllSystemStaffQueryResponse>
        {
            private readonly ISystemStaffService _systemStaffService;

            public GetAllSystemStaffQueryHandler(ISystemStaffService systemStaffService)
            {
                _systemStaffService = systemStaffService;
            }

            public async Task<GetAllSystemStaffQueryResponse> Handle(GetAllSystemStaffQuery request, CancellationToken cancellationToken)
            {
                var result = _systemStaffService.GetAll();
                return new GetAllSystemStaffQueryResponse { SystemStaffs = result };
            }
        }
    }
}