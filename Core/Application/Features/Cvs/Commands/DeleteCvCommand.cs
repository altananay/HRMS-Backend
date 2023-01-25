using Application.Abstractions;
using Application.Results;
using MediatR;
using static Application.Features.Cvs.Commands.DeleteCvCommand;

namespace Application.Features.Cvs.Commands
{
    public partial class DeleteCvCommand : IRequest<DeleteCvCommandResponse>
    {
        public string Id { get; set; }

        public class DeleteCvCommandResponse
        {
            public IResult Result;
        }

        public class DeleteCvCommandHandler : IRequestHandler<DeleteCvCommand, DeleteCvCommandResponse>
        {
            private readonly ICVService _cvService;

            public DeleteCvCommandHandler(ICVService cvService)
            {
                _cvService = cvService;
            }

            public async Task<DeleteCvCommandResponse> Handle(DeleteCvCommand request, CancellationToken cancellationToken)
            {
                var result = await _cvService.Delete(request.Id);
                return new DeleteCvCommandResponse { Result = result };
            }
        }
    }
}