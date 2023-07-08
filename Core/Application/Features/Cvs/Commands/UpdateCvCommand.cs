using Application.Abstractions;
using Application.Results;
using Application.Utilities.Dtos;
using Domain.Objects;
using MediatR;
using static Application.Features.Cvs.Commands.UpdateCvCommand;

namespace Application.Features.Cvs.Commands
{
    public partial class UpdateCvCommand : IRequest<UpdateCvCommandResponse>
    {
        public string Id { get; set; }
        public string JobSeekerId { get; set; }
        public Education[] Educations { get; set; }
        public UpdateJobExperienceDto[]? JobExperiences { get; set; }
        public string[] Skills { get; set; }
        public Language[]? Languages { get; set; }
        public UpdateProjectDto[]? Projects { get; set; }
        public string? ImageUrl { get; set; }
        public SocialMedia? SocialMedias { get; set; }
        public string Information { get; set; }
        public string? Hobbies { get; set; }

        public class UpdateCvCommandResponse
        {
            public IResult Result;
        }

        public class UpdateCvCommandHandler : IRequestHandler<UpdateCvCommand, UpdateCvCommandResponse>
        {
            private readonly ICVService _cvService;

            public UpdateCvCommandHandler(ICVService cvService)
            {
                _cvService = cvService;
            }

            public async Task<UpdateCvCommandResponse> Handle(UpdateCvCommand request, CancellationToken cancellationToken)
            {
                var result = await _cvService.Update(request);
                return new UpdateCvCommandResponse { Result = result };
            }
        }
    }
}
