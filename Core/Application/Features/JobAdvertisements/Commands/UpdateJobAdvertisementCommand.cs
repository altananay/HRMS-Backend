using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobAdvertisements.Commands.UpdateJobAdvertisementCommand;

namespace Application.Features.JobAdvertisements.Commands
{
    public partial class UpdateJobAdvertisementCommand : IRequest<UpdateJobAdvertisementCommandResponse>
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string JobPositionId { get; set; }
        public string JobPositionName { get; set; }
        public string Description { get; set; }
        public string Experience { get; set; }
        public string City { get; set; }
        public string[] Skills { get; set; }
        public double? MinSalary { get; set; }
        public double? MaxSalary { get; set; }
        public int OpenPosition { get; set; }
        public string JobType { get; set; }
        public DateTime Deadline { get; set; }
        public bool Status { get; set; }

        public class UpdateJobAdvertisementCommandResponse
        {
            public IResult Result { get; set; }
        }

        public class UpdateJobAdvertisementCommandHandler : IRequestHandler<UpdateJobAdvertisementCommand, UpdateJobAdvertisementCommandResponse>
        {
            private readonly IJobAdvertisementService _jobAdvertisementService;

            public UpdateJobAdvertisementCommandHandler(IJobAdvertisementService jobAdvertisementService)
            {
                _jobAdvertisementService = jobAdvertisementService;
            }

            public async Task<UpdateJobAdvertisementCommandResponse> Handle(UpdateJobAdvertisementCommand request, CancellationToken cancellationToken)
            {
                var result = await _jobAdvertisementService.Update(request);
                return new UpdateJobAdvertisementCommandResponse { Result = result };
            }
        }
    }
}