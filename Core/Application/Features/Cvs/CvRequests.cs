using Application.Abstractions.Services;
using Application.Common.Contracts;
using Application.Common.Models;
using Application.Results;
using MediatR;

namespace Application.Features.Cvs.Commands
{
    public partial class CreateCvCommand : IRequest<CreateCvCommand.Response>
    {
        public Guid JobSeekerId { get; set; }

        public string? Information { get; set; }
        public string? ImageUrl { get; set; }
        public string? Hobbies { get; set; }
        public string[] Skills { get; set; } = [];
        public SocialMediaRequest? SocialMedia { get; set; }
        public List<EducationRequest> Educations { get; set; } = [];
        public List<JobExperienceRequest> JobExperiences { get; set; } = [];
        public List<CvLanguageRequest> Languages { get; set; } = [];
        public List<CvProjectRequest> Projects { get; set; } = [];

        public sealed class Response
        {
            public IDataResult<CreatedResponse> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<CreateCvCommand, Response>
        {
            private readonly ICvService _cvService;

            public Handler(ICvService cvService) => _cvService = cvService;

            public async Task<Response> Handle(CreateCvCommand request, CancellationToken cancellationToken)
                => new() { Result = await _cvService.AddAsync(request, cancellationToken) };
        }
    }

    public partial class UpdateCvCommand : IRequest<UpdateCvCommand.Response>
    {
        public Guid Id { get; set; }
        public Guid JobSeekerId { get; set; }

        public string? Information { get; set; }
        public string? ImageUrl { get; set; }
        public string? Hobbies { get; set; }
        public string[] Skills { get; set; } = [];
        public SocialMediaRequest? SocialMedia { get; set; }
        public List<EducationRequest> Educations { get; set; } = [];
        public List<JobExperienceRequest> JobExperiences { get; set; } = [];
        public List<CvLanguageRequest> Languages { get; set; } = [];
        public List<CvProjectRequest> Projects { get; set; } = [];

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

        public Guid RequestedBy { get; set; }

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<DeleteCvCommand, Response>
        {
            private readonly ICvService _cvService;

            public Handler(ICvService cvService) => _cvService = cvService;

            public async Task<Response> Handle(DeleteCvCommand request, CancellationToken cancellationToken)
                => new() { Result = await _cvService.DeleteAsync(request.Id, request.RequestedBy, cancellationToken) };
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
            public IDataResult<PagedResult<CvResponse>> Result { get; init; } = null!;
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

        public Guid RequestedBy { get; set; }

        public sealed class Response
        {
            public IDataResult<CvResponse> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetByJobSeekerIdCvQuery, Response>
        {
            private readonly ICvService _cvService;

            public Handler(ICvService cvService) => _cvService = cvService;

            public async Task<Response> Handle(GetByJobSeekerIdCvQuery request, CancellationToken cancellationToken)
                => new() { Result = await _cvService.GetByJobSeekerIdAsync(request.JobSeekerId, request.RequestedBy, cancellationToken) };
        }
    }
}
