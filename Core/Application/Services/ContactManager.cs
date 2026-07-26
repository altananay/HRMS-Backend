using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Common.Dtos;
using Application.Common.Models;
using Application.Features.Contacts.Commands;
using Application.Features.JobPositions.Commands;
using Application.Mapping;
using Application.Results;
using Application.Rules;
using Application.Utilities.Constants;
using Domain.Entities;

namespace Application.Services
{
    public sealed class ContactManager : IContactService
    {
        private readonly IContactRepository _contacts;
        private readonly IUnitOfWork _unitOfWork;
        private readonly BusinessRules _rules;

        public ContactManager(IContactRepository contacts, IUnitOfWork unitOfWork, BusinessRules rules)
        {
            _contacts = contacts;
            _unitOfWork = unitOfWork;
            _rules = rules;
        }

        public async Task<IDataResult<PagedResult<ContactDto>>> GetPagedAsync(
            PageRequest page,
            CancellationToken cancellationToken = default)
        {
            var result = await _contacts.GetPagedAsync(page, cancellationToken);

            return new SuccessDataResult<PagedResult<ContactDto>>(new PagedResult<ContactDto>(
                result.Items.Select(DomainMapper.ToDto).ToList(),
                result.Page,
                result.PageSize,
                result.TotalCount));
        }

        public async Task<IDataResult<ContactDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var contact = await _rules.EnsureContactExistsAsync(id, cancellationToken);
            return new SuccessDataResult<ContactDto>(DomainMapper.ToDto(contact));
        }

        public async Task<IDataResult<CreatedDto>> AddAsync(CreateContactCommand command, CancellationToken cancellationToken = default)
        {
            // Held in a local so the id can be returned: BaseEntity assigns it in the constructor,
            // so it is known before the insert rather than read back after it.
            var contact = new Contact
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                Email = command.Email,
                Subject = command.Subject,
                Message = command.Message
            };

            _contacts.Add(contact);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessDataResult<CreatedDto>(new CreatedDto(contact.Id), Messages.Contact.Added);
        }

        public async Task<IResult> UpdateAsync(UpdateContactCommand command, CancellationToken cancellationToken = default)
        {
            var contact = await _rules.EnsureContactExistsAsync(command.Id, cancellationToken);

            // Change tracking writes only the properties that actually differ. The old flow read the
            // document, hand-copied every unchanged field onto a new object and called
            // ReplaceOneAsync — which is how fields like JobAdvertisement.Status got dropped.
            contact.FirstName = command.FirstName;
            contact.LastName = command.LastName;
            contact.Email = command.Email;
            contact.Subject = command.Subject;
            contact.Message = command.Message;
            contact.IsHandled = command.IsHandled;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.Contact.Updated);
        }

        public async Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var contact = await _rules.EnsureContactExistsAsync(id, cancellationToken);

            _contacts.Remove(contact);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.Contact.Deleted);
        }
    }

    public sealed class JobPositionManager : IJobPositionService
    {
        private readonly IJobPositionRepository _positions;
        private readonly IUnitOfWork _unitOfWork;
        private readonly BusinessRules _rules;

        public JobPositionManager(IJobPositionRepository positions, IUnitOfWork unitOfWork, BusinessRules rules)
        {
            _positions = positions;
            _unitOfWork = unitOfWork;
            _rules = rules;
        }

        public async Task<IDataResult<PagedResult<JobPositionDto>>> GetPagedAsync(
            PageRequest page,
            CancellationToken cancellationToken = default)
        {
            var result = await _positions.GetPagedAsync(page, cancellationToken);

            return new SuccessDataResult<PagedResult<JobPositionDto>>(new PagedResult<JobPositionDto>(
                result.Items.Select(DomainMapper.ToDto).ToList(),
                result.Page,
                result.PageSize,
                result.TotalCount));
        }

        public async Task<IDataResult<JobPositionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var position = await _rules.EnsureJobPositionExistsAsync(id, cancellationToken);
            return new SuccessDataResult<JobPositionDto>(DomainMapper.ToDto(position));
        }

        public async Task<IDataResult<CreatedDto>> AddAsync(CreateJobPositionCommand command, CancellationToken cancellationToken = default)
        {
            // Resolve-or-create rather than blind insert: the name is unique now, and an admin
            // re-adding an existing position should be idempotent rather than a 500 from the index.
            // The returned id is therefore the existing position's when the name was already taken,
            // which is what makes the call idempotent in the response as well as in the table.
            var position = await _positions.ResolveOrCreateAsync(command.Name, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessDataResult<CreatedDto>(new CreatedDto(position.Id), Messages.JobPosition.Added);
        }

        public async Task<IResult> UpdateAsync(UpdateJobPositionCommand command, CancellationToken cancellationToken = default)
        {
            var position = await _rules.EnsureJobPositionExistsAsync(command.Id, cancellationToken);

            position.Name = command.Name.Trim();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.JobPosition.Updated);
        }

        public async Task<IResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var position = await _rules.EnsureJobPositionExistsAsync(id, cancellationToken);

            // Now that positions are shared, deleting one that advertisements reference would break
            // them. The FK is RESTRICT; this turns that into a 409 with a usable message.
            await _rules.EnsureJobPositionNotReferencedAsync(id, cancellationToken);

            _positions.Remove(position);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.JobPosition.Deleted);
        }
    }
}
