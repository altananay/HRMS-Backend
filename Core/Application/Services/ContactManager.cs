using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Common.Contracts;
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

        public async Task<IDataResult<PagedResult<ContactResponse>>> GetPagedAsync(
            PageRequest page,
            CancellationToken cancellationToken = default)
        {
            var result = await _contacts.GetPagedAsync(page, cancellationToken);

            return new SuccessDataResult<PagedResult<ContactResponse>>(new PagedResult<ContactResponse>(
                result.Items.Select(DomainMapper.ToResponse).ToList(),
                result.Page,
                result.PageSize,
                result.TotalCount));
        }

        public async Task<IDataResult<ContactResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var contact = await _rules.EnsureContactExistsAsync(id, cancellationToken);
            return new SuccessDataResult<ContactResponse>(DomainMapper.ToResponse(contact));
        }

        public async Task<IDataResult<CreatedResponse>> AddAsync(CreateContactCommand command, CancellationToken cancellationToken = default)
        {
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

            return new SuccessDataResult<CreatedResponse>(new CreatedResponse(contact.Id), Messages.Contact.Added);
        }

        public async Task<IResult> UpdateAsync(UpdateContactCommand command, CancellationToken cancellationToken = default)
        {
            var contact = await _rules.EnsureContactExistsAsync(command.Id, cancellationToken);

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

        public async Task<IDataResult<PagedResult<JobPositionResponse>>> GetPagedAsync(
            PageRequest page,
            CancellationToken cancellationToken = default)
        {
            var result = await _positions.GetPagedAsync(page, cancellationToken);

            return new SuccessDataResult<PagedResult<JobPositionResponse>>(new PagedResult<JobPositionResponse>(
                result.Items.Select(DomainMapper.ToResponse).ToList(),
                result.Page,
                result.PageSize,
                result.TotalCount));
        }

        public async Task<IDataResult<JobPositionResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var position = await _rules.EnsureJobPositionExistsAsync(id, cancellationToken);
            return new SuccessDataResult<JobPositionResponse>(DomainMapper.ToResponse(position));
        }

        public async Task<IDataResult<CreatedResponse>> AddAsync(CreateJobPositionCommand command, CancellationToken cancellationToken = default)
        {
            var position = await _positions.ResolveOrCreateAsync(command.Name, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessDataResult<CreatedResponse>(new CreatedResponse(position.Id), Messages.JobPosition.Added);
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

            await _rules.EnsureJobPositionNotReferencedAsync(id, cancellationToken);

            _positions.Remove(position);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.JobPosition.Deleted);
        }
    }
}
