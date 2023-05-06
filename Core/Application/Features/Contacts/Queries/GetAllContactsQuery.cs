using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.Contacts.Queries.GetAllContactsQuery;

namespace Application.Features.Contacts.Queries
{
    public partial class GetAllContactsQuery : IRequest<GetAllContactsQueryResponse>
    {
        public class GetAllContactsQueryResponse
        {
            public IDataResult<IQueryable<Contact>> Result { get; set; }
        }

        public class GetAllContactsQueryHandler : IRequestHandler<GetAllContactsQuery, GetAllContactsQueryResponse>
        {
            private readonly IContactService _contactService;

            public GetAllContactsQueryHandler(IContactService contactService)
            {
                _contactService = contactService;
            }

            public async Task<GetAllContactsQueryResponse> Handle(GetAllContactsQuery request, CancellationToken cancellationToken)
            {
                var result = _contactService.GetAll();
                return new GetAllContactsQueryResponse { Result = result };
            }
        }
    }
}