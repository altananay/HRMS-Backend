namespace Application.Common.Models
{
    public sealed record JobAdvertisementFilter
    {
        public Guid? EmployerId { get; init; }

        public bool? IsActive { get; init; }

        public string? Skill { get; init; }

        public string? City { get; init; }

        public string? Search { get; init; }

        public bool OrderByHighestSalary { get; init; }

        public static JobAdvertisementFilter None { get; } = new();
    }
}
