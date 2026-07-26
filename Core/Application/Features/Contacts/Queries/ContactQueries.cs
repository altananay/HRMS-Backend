using Application.Abstractions.Services;
using Application.Common.Contracts;
using Application.Common.Models;
using Application.Results;
using MediatR;

namespace Application.Features.Contacts.Queries
{
    public partial class GetAllContactsQuery : IRequest<GetAllContactsQuery.Response>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = PageRequest.DefaultPageSize;

        public sealed class Response
        {
            public IDataResult<PagedResult<ContactResponse>> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetAllContactsQuery, Response>
        {
            private readonly IContactService _contactService;

            public Handler(IContactService contactService) => _contactService = contactService;

            public async Task<Response> Handle(GetAllContactsQuery request, CancellationToken cancellationToken)
                => new()
                {
                    Result = await _contactService.GetPagedAsync(
                        new PageRequest(request.Page, request.PageSize), cancellationToken)
                };
        }
    }

    public partial class GetByIdContactQuery : IRequest<GetByIdContactQuery.Response>
    {
        public Guid Id { get; set; }

        public sealed class Response
        {
            public IDataResult<ContactResponse> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetByIdContactQuery, Response>
        {
            private readonly IContactService _contactService;

            public Handler(IContactService contactService) => _contactService = contactService;

            public async Task<Response> Handle(GetByIdContactQuery request, CancellationToken cancellationToken)
                => new() { Result = await _contactService.GetByIdAsync(request.Id, cancellationToken) };
        }
    }
}
