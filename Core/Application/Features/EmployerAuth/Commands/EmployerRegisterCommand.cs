using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.EmployerAuth.Commands.EmployerRegisterCommand;

namespace Application.Features.EmployerAuth.Commands
{
    public partial class EmployerRegisterCommand : IRequest<EmployerRegisterCommandResponse>
    {
        public string CompanyName { get; set; }
        public string CompanyPhone { get; set; }
        public string WebSite { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public class EmployerRegisterCommandResponse
        {
            public IResult Employer;
        }

        public class EmployerRegisterCommandHandler : IRequestHandler<EmployerRegisterCommand, EmployerRegisterCommandResponse>
        {
            private IEmployerAuthService _authService;

            public EmployerRegisterCommandHandler(IEmployerAuthService authService)
            {
                _authService = authService;
            }

            public async Task<EmployerRegisterCommandResponse> Handle(EmployerRegisterCommand request, CancellationToken cancellationToken)
            {
                var userExists = _authService.UserExists(request.Email);
                if (!userExists.IsSuccess)
                {
                    return new EmployerRegisterCommandResponse { Employer = null };
                }

                var registerResult = await _authService.Register(request, request.Password);
                return new EmployerRegisterCommandResponse { Employer = registerResult };
            }
        }
    }
}