using Application.Abstractions.Services;
using Application.Common.Dtos;
using Application.Results;
using MediatR;

namespace Application.Features.Contacts.Commands
{
    /// <summary>
    /// Contact write requests.
    /// </summary>
    /// <remarks>
    /// Requests keep the documented shape — a request type with its <c>Response</c> and
    /// <c>Handler</c> nested inside it, and a handler that only delegates to an <c>I*Service</c>.
    /// The one deviation from the previous layout is that a module's commands share a file instead
    /// of taking one file each; with ~50 requests across 8 modules the per-file split was mostly
    /// using-directive boilerplate, and grouping keeps a module's write surface readable at a glance.
    ///
    /// Two shape fixes carried across every module: response members are properties rather than
    /// public fields, and each response exposes its payload as <c>Result</c> rather than a
    /// differently-named member per module (<c>JobAdvertisements</c>, <c>Users</c>, <c>Contact</c>,
    /// <c>DataResult</c>…), which is what stopped the controllers from sharing any common handling.
    /// </remarks>
    public partial class CreateContactCommand : IRequest<CreateContactCommand.Response>
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Message { get; set; } = null!;

        public sealed class Response
        {
            // Declared as the concrete result type, not IResult. System.Text.Json serializes by the
            // declared type, so an IResult-typed member would emit isSuccess and message and quietly
            // drop the data payload — the id would never reach the client.
            public IDataResult<CreatedDto> Result { get; init; } = null!;
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
