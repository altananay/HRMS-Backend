using Application.Abstractions;
using Application.Results;
using MediatR;
using static Application.Features.Employers.Commands.DeleteEmployerCommand;

namespace Application.Features.Employers.Commands
{
    public partial class DeleteEmployerCommand : IRequest<DeleteEmployerCommandResponse>
    {
        public string Id { get; set; }

        public class DeleteEmployerCommandResponse
        {
            public IResult Result;
        }

        public class DeleteEmployerCommandHandler : IRequestHandler<DeleteEmployerCommand, DeleteEmployerCommandResponse>
        {
            private readonly IEmployerService _employerService;

            public DeleteEmployerCommandHandler(IEmployerService employerService)
            {
                _employerService = employerService;
            }

            public async Task<DeleteEmployerCommandResponse> Handle(DeleteEmployerCommand request, CancellationToken cancellationToken)
            {
                var result = await _employerService.Delete(request.Id);
                return new DeleteEmployerCommandResponse { Result = result };
            }
        }
    }
}