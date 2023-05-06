using Application.Abstractions;
using Application.Results;
using MediatR;
using static Application.Features.Contacts.Commands.DeleteContactCommand;

namespace Application.Features.Contacts.Commands
{
    public partial class DeleteContactCommand : IRequest<DeleteContactResponse>
    {
        public string Id { get; set; }

        public class DeleteContactResponse
        {
            public IResult Result { get; set; }
        }

        public class DeleteContactHandler : IRequestHandler<DeleteContactCommand, DeleteContactResponse>
        {
            private readonly IContactService _contactService;

            public DeleteContactHandler(IContactService contactService)
            {
                _contactService = contactService;
            }

            public async Task<DeleteContactResponse> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
            {
                var result = await _contactService.DeleteAsync(request.Id);
                return new DeleteContactResponse { Result = result };
            }
        }
    }
}