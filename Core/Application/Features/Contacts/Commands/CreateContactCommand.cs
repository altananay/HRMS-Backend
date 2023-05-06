using Application.Abstractions;
using Application.Results;
using MediatR;
using static Application.Features.Contacts.Commands.CreateContactCommand;

namespace Application.Features.Contacts.Commands
{
    public partial class CreateContactCommand : IRequest<CreateContactResponse>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }

        public class CreateContactResponse
        {
            public IResult Result { get; set; }
        }

        public class CreateContactHandler : IRequestHandler<CreateContactCommand, CreateContactResponse>
        {
            private readonly IContactService _contactService;

            public CreateContactHandler(IContactService contactService)
            {
                _contactService = contactService;
            }

            public async Task<CreateContactResponse> Handle(CreateContactCommand request, CancellationToken cancellationToken)
            {
                var result = await _contactService.AddAsync(request);
                return new CreateContactResponse { Result = result };
            }
        }
    }
}