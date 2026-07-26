using Application.Abstractions.Services;
using Application.Common.Contracts;
using Application.Results;
using MediatR;

namespace Application.Features.Contacts.Commands
{
    public partial class CreateContactCommand : IRequest<CreateContactCommand.Response>
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Message { get; set; } = null!;

        public sealed class Response
        {
            // Declared type, not IResult: System.Text.Json serializes by it, so IResult here would
            // drop the data payload and the created id would never reach the client.
            public IDataResult<CreatedResponse> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<CreateContactCommand, Response>
        {
            private readonly IContactService _contactService;

            public Handler(IContactService contactService) => _contactService = contactService;

            public async Task<Response> Handle(CreateContactCommand request, CancellationToken cancellationToken)
                => new() { Result = await _contactService.AddAsync(request, cancellationToken) };
        }
    }

    public partial class UpdateContactCommand : IRequest<UpdateContactCommand.Response>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsHandled { get; set; }

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<UpdateContactCommand, Response>
        {
            private readonly IContactService _contactService;

            public Handler(IContactService contactService) => _contactService = contactService;

            public async Task<Response> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
                => new() { Result = await _contactService.UpdateAsync(request, cancellationToken) };
        }
    }

    public partial class DeleteContactCommand : IRequest<DeleteContactCommand.Response>
    {
        public Guid Id { get; set; }

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<DeleteContactCommand, Response>
        {
            private readonly IContactService _contactService;

            public Handler(IContactService contactService) => _contactService = contactService;

            public async Task<Response> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
                => new() { Result = await _contactService.DeleteAsync(request.Id, cancellationToken) };
        }
    }
}
