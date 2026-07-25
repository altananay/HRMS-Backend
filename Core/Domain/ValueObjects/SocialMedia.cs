namespace Domain.ValueObjects
{
    /// <summary>
    /// Social links on a CV. Mapped as an EF Core owned type — three columns on <c>cvs</c>.
    /// </summary>
    /// <remarks>
    /// The property on <c>Cv</c> was named <c>SocialMedias</c> (plural) but held a single object.
    /// Renamed to singular to match what it actually is.
    /// </remarks>
    public class SocialMedia
    {
        public string? Github { get; set; }

        public string? Linkedin { get; set; }

        public string? WebSite { get; set; }
    }
}
