using Domain.Enums;

namespace Application.Common.Models
{
    public sealed record JobApplicationFilter
    {
        public Guid? EmployerId { get; init; }

        public Guid? JobSeekerId { get; init; }

        public Guid? JobAdvertisementId { get; init; }

        public JobApplicationStatus? Status { get; init; }

        public static JobApplicationFilter None { get; } = new();
    }
}
