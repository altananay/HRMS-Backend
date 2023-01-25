using Application.Abstractions;
using Application.Results;
using MediatR;
using static Application.Features.JobAdvertisements.Commands.DeleteJobAdvertisementCommand;

namespace Application.Features.JobAdvertisements.Commands
{
    public partial class DeleteJobAdvertisementCommand : IRequest<DeleteJobAdvertisementCommandResponse>
    {
        public string Id { get; set; }

        public class DeleteJobAdvertisementCommandResponse
        {
            public IResult Result { get; set; }
        }

        public class DeleteJobAdvertisementCommandHandler : IRequestHandler<DeleteJobAdvertisementCommand, DeleteJobAdvertisementCommandResponse>
        {
            private readonly IJobAdvertisementService _jobAdvertisementService;

            public DeleteJobAdvertisementCommandHandler(IJobAdvertisementService jobAdvertisementService)
            {
                _jobAdvertisementService = jobAdvertisementService;
            }

            public async Task<DeleteJobAdvertisementCommandResponse> Handle(DeleteJobAdvertisementCommand request, CancellationToken cancellationToken)
            {
                var result = await _jobAdvertisementService.Delete(request.Id);
                return new DeleteJobAdvertisementCommandResponse { Result = result };
            }
        }
    }
}