namespace Domain.Common
{
    /// <summary>
    /// Base for every persisted entity.
    /// </summary>
    /// <remarks>
    /// The key is a client-generated UUID v7 rather than the previous <c>string</c> holding a Mongo
    /// ObjectId. Three reasons it is generated here in the constructor rather than by the database:
    ///
    /// <list type="bullet">
    /// <item>An entity has its identity before <c>SaveChangesAsync</c>, so a whole object graph
    /// (JobSeeker + Cv + Educations) can be built and saved in one round trip.</item>
    /// <item>Version 7 is time-ordered, so inserts append to the right of the B-tree instead of
    /// scattering across it the way random v4 GUIDs do.</item>
    /// <item>Tests can assert against known ids without a lookup.</item>
    /// </list>
    ///
    /// Sequential integers were rejected deliberately: this is a public job board, and incrementing
    /// ids would leak record counts and make every job seeker and application trivially enumerable.
    /// </remarks>
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>Set by AuditingSaveChangesInterceptor from the injected TimeProvider.</summary>
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
