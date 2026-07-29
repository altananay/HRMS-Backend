namespace Application.Common.Models
{
    /// <summary>
    /// The optional narrowing applied to a job advertisement listing.
    /// </summary>
    /// <remarks>
    /// A record rather than six optional parameters: at that count a positional call site stops being
    /// readable and two same-typed arguments can be swapped without the compiler noticing.
    /// </remarks>
    public sealed record JobAdvertisementFilter
    {
        public Guid? EmployerId { get; init; }

        public bool? IsActive { get; init; }

        /// <summary>Exact match against an element of the <c>skills</c> array (GIN-indexed).</summary>
        public string? Skill { get; init; }

        /// <summary>Case-insensitive match on the whole city name.</summary>
        public string? City { get; init; }

        /// <summary>Case-insensitive substring of the title or the description.</summary>
        public string? Search { get; init; }

        public bool OrderByHighestSalary { get; init; }

        public static JobAdvertisementFilter None { get; } = new();
    }
}
