using Application.Abstractions;
using Application.Results;
using MediatR;
using static Application.Features.Contacts.Commands.UpdateContactCommand;

namespace Application.Features.Contacts.Commands
{
    public partial class UpdateContactCommand : IRequest<UpdateContactResponse>
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }

        public class UpdateContactResponse
        {
            public IResult Result { get; set; }
        }

        public class UpdateContactHandler : IRequestHandler<UpdateContactCommand, UpdateContactResponse>
        {
            private readonly IContactService _contactService;

            public UpdateContactHandler(IContactService contactService)
            {
                _contactService = contactService;
            }

            public async Task<UpdateContactResponse> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
            {
                var result = await _contactService.UpdateAsync(request);
                return new UpdateContactResponse { Result = result };
            }
        }
    }
}