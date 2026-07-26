using Application.Abstractions.Services;
using Application.Abstractions.Storage;
using Application.Common.Contracts;
using Application.Results;
using MediatR;

namespace Application.Features.Cvs.Commands
{
    public partial class UploadCvFileCommand : IRequest<UploadCvFileCommand.Response>
    {
        public Guid JobSeekerId { get; set; }

        public IReadOnlyList<FileUploadRequest> Files { get; set; } = [];

        public sealed class Response
        {
            public IDataResult<IReadOnlyList<CvFileResponse>> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<UploadCvFileCommand, Response>
        {
            private readonly ICvFileService _cvFileService;

            public Handler(ICvFileService cvFileService) => _cvFileService = cvFileService;

            public async Task<Response> Handle(UploadCvFileCommand request, CancellationToken cancellationToken)
                => new() { Result = await _cvFileService.UploadAsync(request, cancellationToken) };
        }
    }

    public partial class DownloadCvFileQuery : IRequest<DownloadCvFileQuery.Response>
    {
        public Guid Id { get; set; }
        public Guid RequestedBy { get; set; }

        public sealed class Response
        {
            public CvFileDownload Download { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<DownloadCvFileQuery, Response>
        {
            private readonly ICvFileService _cvFileService;

            public Handler(ICvFileService cvFileService) => _cvFileService = cvFileService;

            public async Task<Response> Handle(DownloadCvFileQuery request, CancellationToken cancellationToken)
                => new() { Download = await _cvFileService.DownloadAsync(request.Id, request.RequestedBy, cancellationToken) };
        }
    }

    public partial class DeleteCvFileCommand : IRequest<DeleteCvFileCommand.Response>
    {
        public Guid Id { get; set; }
        public Guid RequestedBy { get; set; }

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<DeleteCvFileCommand, Response>
        {
            private readonly ICvFileService _cvFileService;

            public Handler(ICvFileService cvFileService) => _cvFileService = cvFileService;

            public async Task<Response> Handle(DeleteCvFileCommand request, CancellationToken cancellationToken)
                => new() { Result = await _cvFileService.DeleteAsync(request.Id, request.RequestedBy, cancellationToken) };
        }
    }
}
