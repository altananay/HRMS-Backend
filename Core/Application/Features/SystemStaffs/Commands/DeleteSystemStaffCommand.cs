using Application.Abstractions;
using Application.Results;
using MediatR;
using static Application.Features.SystemStaffs.Commands.DeleteSystemStaffCommand;

namespace Application.Features.SystemStaffs.Commands
{
    public partial class DeleteSystemStaffCommand : IRequest<DeleteSystemStaffCommandResponse>
    {
        public string Id { get; set; }

        public class DeleteSystemStaffCommandResponse
        {
            public IResult Result;
        }

        public class DeleteSystemStaffCommandHandler : IRequestHandler<DeleteSystemStaffCommand, DeleteSystemStaffCommandResponse>
        {
            private readonly ISystemStaffService _systemStaffService;

            public DeleteSystemStaffCommandHandler(ISystemStaffService systemStaffService)
            {
                _systemStaffService = systemStaffService;
            }

            public async Task<DeleteSystemStaffCommandResponse> Handle(DeleteSystemStaffCommand request, CancellationToken cancellationToken)
            {
                var result = await _systemStaffService.Delete(request.Id);
                return new DeleteSystemStaffCommandResponse { Result = result };
            }
        }
    }
}