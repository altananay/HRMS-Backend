﻿using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobAdvertisements.Commands.CreateJobAdvertisementCommand;

namespace Application.Features.JobAdvertisements.Commands
{
    public partial class CreateJobAdvertisementCommand : IRequest<CreateJobAdvertisementCommandResponse>
    {
        public string EmployerId { get; set; }
        public string Title { get; set; }
        public string JobPositionName { get; set; }
        public string Description { get; set; }
        public string Experience { get; set; }
        public string City { get; set; }
        public string[] Skills { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string Currency { get; set; }
        public int OpenPosition { get; set; }
        public string JobType { get; set; }
        public DateTime Deadline { get; set; }

        public class CreateJobAdvertisementCommandResponse
        {
            public IResult Result;
        }

        public class CreateJobAdvertisementCommandHandler : IRequestHandler<CreateJobAdvertisementCommand, CreateJobAdvertisementCommandResponse>
        {
            private readonly IJobAdvertisementService _jobAdvertisementService;

            public CreateJobAdvertisementCommandHandler(IJobAdvertisementService jobAdvertisementService)
            {
                _jobAdvertisementService = jobAdvertisementService;
            }

            public async Task<CreateJobAdvertisementCommandResponse> Handle(CreateJobAdvertisementCommand request, CancellationToken cancellationToken)
            {
                var result = await _jobAdvertisementService.Add(request);
                return new CreateJobAdvertisementCommandResponse { Result = result };
            }
        }
    }
}