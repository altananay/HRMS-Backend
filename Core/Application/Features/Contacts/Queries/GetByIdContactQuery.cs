using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.Contacts.Queries.GetByIdContactQuery;

namespace Application.Features.Contacts.Queries
{
    public class GetByIdContactQuery : IRequest<GetByIdContactQueryResponse>
    {
        public string Id { get; set; }

        public class GetByIdContactQueryResponse
        {
            public IDataResult<Contact> Contact { get; set; }
        }

        public class GetByIdContactQueryHandler : IRequestHandler<GetByIdContactQuery, GetByIdContactQueryResponse>
        {
            private readonly IContactService _contactService;

            public GetByIdContactQueryHandler(IContactService contactService)
            {
                _contactService = contactService;
            }

            public async Task<GetByIdContactQueryResponse> Handle(GetByIdContactQuery request, CancellationToken cancellationToken)
            {
                var result = _contactService.GetById(request.Id);
                return new GetByIdContactQueryResponse { Contact = result };
            }
        }
    }
}