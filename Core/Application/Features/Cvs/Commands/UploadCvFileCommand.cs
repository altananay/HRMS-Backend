using Application.Abstractions.Storage;
using Application.Repositories.CvFiles;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using static Application.Features.Cvs.Commands.UploadCvFileCommand;

namespace Application.Features.Cvs.Commands
{
    public class UploadCvFileCommand : IRequest<UploadCvFileCommandResponse>
    {
        public IFormFileCollection? Files { get; set; }

        public class UploadCvFileCommandResponse
        {

        }

        public class UploadCvFileCommandHandler : IRequestHandler<UploadCvFileCommand, UploadCvFileCommandResponse>
        {
            private readonly IStorageService _storageService;
            private readonly ICvFileWriteRepository _cvFileWriteRepository;

            public UploadCvFileCommandHandler(IStorageService storageService, ICvFileWriteRepository cvFileWriteRepository)
            {
                _storageService = storageService;
                _cvFileWriteRepository = cvFileWriteRepository;
            }

            public async Task<UploadCvFileCommandResponse> Handle(UploadCvFileCommand request, CancellationToken cancellationToken)
            {
                var datas = await _storageService.UploadAsync("files", request.Files);
                await _cvFileWriteRepository.AddRangeAsync(datas.Select(d => new CvFile()
                {
                    FileName = d.fileName,
                    Path = d.pathOrContainerName,
                    Storage = _storageService.StorageName
                }).ToList());
                return new();
            }
        }
    }
}