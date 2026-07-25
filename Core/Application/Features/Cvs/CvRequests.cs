using Application.Abstractions.Services;
using Application.Common.Dtos;
using Application.Common.Models;
using Application.Results;
using MediatR;

namespace Application.Features.Cvs.Commands
{
    public partial class CreateCvCommand : IRequest<CreateCvCommand.Response>
    {
        /// <summary>Taken from the authenticated seeker's token, not the request body.</summary>
        public Guid JobSeekerId { get; set; }

        public string? Information { get; set; }
        public string? ImageUrl { get; set; }
        public string? Hobbies { get; set; }
        public string[] Skills { get; set; } = [];
        public SocialMediaInput? SocialMedia { get; set; }
        public List<EducationInput> Educations { get; set; } = [];
        public List<JobExperienceInput> JobExperiences { get; set; } = [];
        public List<CvLanguageInput> Languages { get; set; } = [];
        public List<CvProjectInput> Projects { get; set; } = [];

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<CreateCvCommand, Response>
        {
            private readonly ICvService _cvService;

            public Handler(ICvService cvService) => _cvService = cvService;

            public async Task<Response> Handle(CreateCvCommand request, CancellationToken cancellationToken)
                => new() { Result = await _cvService.AddAsync(request, cancellationToken) };
        }
    }

    /// <remarks>
    /// This endpoint could never succeed before. <c>CvManager.Update</c> called
    /// <c>CheckIfCvExistsByJobSeekerId</c>, a guard that throws when a CV <i>is</i> found — so
    /// updating an existing CV threw <c>BusinessException</c> every time and surfaced as HTTP 500.
    /// </remarks>
    public partial class UpdateCvCommand : IRequest<UpdateCvCommand.Response>
    {
        public Guid Id { get; set; }
        public Guid JobSeekerId { get; set; }

        public string? Information { get; set; }
        public string? ImageUrl { get; set; }
        public string? Hobbies { get; set; }
        public string[] Skills { get; set; } = [];
        public SocialMediaInput? SocialMedia { get; set; }
        public List<EducationInput> Educations { get; set; } = [];
        public List<JobExperienceInput> JobExperiences { get; set; } = [];
        public List<CvLanguageInput> Languages { get; set; } = [];
        public List<CvProjectInput> Projects { get; set; } = [];

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<UpdateCvCommand, Response>
        {
            private readonly ICvService _cvService;

            public Handler(ICvService cvService) => _cvService = cvService;

            public async Task<Response> Handle(UpdateCvCommand request, CancellationToken cancellationToken)
                => new() { Result = await _cvService.UpdateAsync(request, cancellationToken) };
        }
    }

    public partial class DeleteCvCommand : IRequest<DeleteCvCommand.Response>
    {
        public Guid Id { get; set; }

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<DeleteCvCommand, Response>
        {
            private readonly ICvService _cvService;

            public Handler(ICvService cvService) => _cvService = cvService;

            public async Task<Response> Handle(DeleteCvCommand request, CancellationToken cancellationToken)
                => new() { Result = await _cvService.DeleteAsync(request.Id, cancellationToken) };
        }
    }
}

namespace Application.Features.Cvs.Queries
{
    public partial class GetAllCvQuery : IRequest<GetAllCvQuery.Response>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = PageRequest.DefaultPageSize;

        public sealed class Response
        {
            public IDataResult<PagedResult<CvDto>> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetAllCvQuery, Response>
        {
            private readonly ICvService _cvService;

            public Handler(ICvService cvService) => _cvService = cvService;

            public async Task<Response> Handle(GetAllCvQuery request, CancellationToken cancellationToken)
                => new()
                {
                    Result = await _cvService.GetPagedAsync(
                        new PageRequest(request.Page, request.PageSize), cancellationToken)
                };
        }
    }

    public partial class GetByJobSeekerIdCvQuery : IRequest<GetByJobSeekerIdCvQuery.Response>
    {
        public Guid JobSeekerId { get; set; }

        public sealed class Response
        {
            public IDataResult<CvDto> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetByJobSeekerIdCvQuery, Response>
        {
            private readonly ICvService _cvService;

            public Handler(ICvService cvService) => _cvService = cvService;

            public async Task<Response> Handle(GetByJobSeekerIdCvQuery request, CancellationToken cancellationToken)
                => new() { Result = await _cvService.GetByJobSeekerIdAsync(request.JobSeekerId, cancellationToken) };
        }
    }
}
