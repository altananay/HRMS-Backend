using Application.Abstractions;
using Application.Results;
using MediatR;
using static Application.Features.SystemStaffs.Commands.UpdateSystemStaffCommand;

namespace Application.Features.SystemStaffs.Commands
{
    public partial class UpdateSystemStaffCommand : IRequest<UpdateSystemStaffCommandResponse>
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Status { get; set; }
        public string[] Claims { get; set; }

        public class UpdateSystemStaffCommandResponse
        {
            public IResult Result;
        }

        public class UpdateSystemStaffCommandHandler : IRequestHandler<UpdateSystemStaffCommand, UpdateSystemStaffCommandResponse>
        {
            private readonly ISystemStaffService _systemStaffService;

            public UpdateSystemStaffCommandHandler(ISystemStaffService systemStaffService)
            {
                _systemStaffService = systemStaffService;
            }

            public async Task<UpdateSystemStaffCommandResponse> Handle(UpdateSystemStaffCommand request, CancellationToken cancellationToken)
            {
                var result = await _systemStaffService.UpdateAsync(request);
                return new UpdateSystemStaffCommandResponse { Result = result };
            }
        }

    }
}
