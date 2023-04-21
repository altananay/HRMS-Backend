using Application.Abstractions;
using Application.Dtos;
using Application.Results;
using Domain.Objects;
using MediatR;
using static Application.Features.Cvs.Commands.CreateCvCommand;

namespace Application.Features.Cvs.Commands
{
    public partial class CreateCvCommand : IRequest<CreateCvCommandResponse>
    {
        public string JobSeekerId { get; set; }
        public Education[] Educations { get; set; }
        public CreateJobExperienceDto[] JobExperiences { get; set; }
        public string[] Skills { get; set; }
        public Language[]? Languages { get; set; }
        public CreateProjectDto[] Projects { get; set; }
        public string? ImageUrl { get; set; }
        public SocialMedia? SocialMedias { get; set; }
        public string Information { get; set; }
        public string? Hobbies { get; set; }

        public class CreateCvCommandResponse
        {
            public IResult Result;
        }

        public class CreateCvCommandHandler : IRequestHandler<CreateCvCommand, CreateCvCommandResponse>
        {
            private readonly ICVService _cvService;

            public CreateCvCommandHandler(ICVService cvService)
            {
                _cvService = cvService;
            }

            public async Task<CreateCvCommandResponse> Handle(CreateCvCommand request, CancellationToken cancellationToken)
            {
                var result = await _cvService.Add(request);
                return new CreateCvCommandResponse { Result = result };
            }
        }
    }
}