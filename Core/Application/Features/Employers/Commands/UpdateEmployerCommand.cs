using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.Employers.Commands.UpdateEmployerCommand;

namespace Application.Features.Employers.Commands
{
    public partial class UpdateEmployerCommand : IRequest<UpdateEmployerCommandResponse>
    {
        public string CompanyName { get; set; }
        public string CompanyPhone { get; set; }
        public string WebSite { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string[] Sector { get; set; }
        public string[] Departments { get; set; }
        public string NumberOfEmployees { get; set; }

        public class UpdateEmployerCommandResponse
        {
            public IResult Result;
        }

        public class UpdateEmployerCommandHandler : IRequestHandler<UpdateEmployerCommand, UpdateEmployerCommandResponse>
        {
            private readonly IEmployerService _employerService;

            public UpdateEmployerCommandHandler(IEmployerService employerService)
            {
                _employerService = employerService;
            }

            public async Task<UpdateEmployerCommandResponse> Handle(UpdateEmployerCommand request, CancellationToken cancellationToken)
            {
                var result = await _employerService.Update(request);
                return new UpdateEmployerCommandResponse { Result = result };
            }
        }
    }
}
