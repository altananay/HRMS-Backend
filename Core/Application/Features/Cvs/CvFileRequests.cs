using Application.Abstractions.Services;
using Application.Abstractions.Storage;
using Application.Common.Dtos;
using Application.Results;
using MediatR;

namespace Application.Features.Cvs.Commands
{
    /// <remarks>
    /// The old <c>UploadCvFileCommand</c> bypassed the manager layer entirely, wrote <c>CvFile</c>
    /// rows with no link to any CV or seeker (the entity had no foreign key at all), declared an
    /// empty response type, and had its result discarded by a controller that always returned
    /// <c>Ok()</c>. Every uploaded file was unattributable the moment it was written.
    /// </remarks>
    public partial class UploadCvFileCommand : IRequest<UploadCvFileCommand.Response>
    {
        /// <summary>Set from the authenticated seeker's token, never bound from the request.</summary>
        public Guid JobSeekerId { get; set; }

        /// <summary>Translated from IFormFile by the controller, so Application stays free of ASP.NET.</summary>
        public IReadOnlyList<FileUploadRequest> Files { get; set; } = [];

        public sealed class Response
        {
            public IDataResult<IReadOnlyList<CvFileDto>> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<UploadCvFileCommand, Response>
        {
            private readonly ICvFileService _cvFileService;

            public Handler(ICvFileService cvFileService) => _cvFileService = cvFileService;

            public async Task<Response> Handle(UploadCvFileCommand request, CancellationToken cancellationToken)
                => new() { Result = await _cvFileService.UploadAsync(request, cancellationToken) };
        }
    }

    /// <remarks>
    /// A query would read better, but it lives here beside the other file operations. The response
    /// carries a live <see cref="Stream"/> rather than a Result envelope: the controller streams it
    /// straight to the caller, so it is never buffered into memory.
    /// </remarks>
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
