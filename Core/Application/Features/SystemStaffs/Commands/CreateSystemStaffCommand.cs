using Application.Abstractions;
using Application.Results;
using MediatR;
using static Application.Features.SystemStaffs.Commands.CreateSystemStaffCommand;

namespace Application.Features.SystemStaffs.Commands
{
    public partial class CreateSystemStaffCommand : IRequest<CreateSystemStaffCommandResponse>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Status { get; set; }
        public string[] Claims { get; set; }

        public class CreateSystemStaffCommandResponse
        {
            public IResult Result;
        }

        public class CreateSystemStaffCommandHandler : IRequestHandler<CreateSystemStaffCommand, CreateSystemStaffCommandResponse>
        {
            private readonly ISystemStaffAuthService _systemStaffAuthService;

            public CreateSystemStaffCommandHandler(ISystemStaffAuthService systemStaffAuthService)
            {
                _systemStaffAuthService = systemStaffAuthService;
            }

            public async Task<CreateSystemStaffCommandResponse> Handle(CreateSystemStaffCommand request, CancellationToken cancellationToken)
            {
                var result = await _systemStaffAuthService.Register(request, request.Password);
                return new CreateSystemStaffCommandResponse { Result = result };
            }
        }
    }
}
