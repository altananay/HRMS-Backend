namespace Domain.Enums
{
    /// <summary>
    /// Where an application sits in the hiring pipeline.
    /// </summary>
    /// <remarks>
    /// Replaces the free-text Turkish <c>Result</c> string (e.g. "İş başvurusu yapıldı."), which was
    /// written by the server and rendered verbatim by the client — so the display language was
    /// baked into the database and no state could be queried or filtered reliably.
    ///
    /// Persisted as text, not as an ordinal, so inserting a new member later cannot silently
    /// reinterpret existing rows.
    /// </remarks>
    public enum JobApplicationStatus
    {
        Submitted = 0,
        UnderReview = 1,
        InterviewScheduled = 2,
        Offered = 3,
        Accepted = 4,
        Rejected = 5,
        Withdrawn = 6
    }
}
