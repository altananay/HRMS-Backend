namespace Domain.Enums
{
    /// <summary>
    /// Which concrete actor a <c>User</c> row represents.
    /// </summary>
    /// <remarks>
    /// This duplicates information already implied by which TPT table holds the row, and that
    /// redundancy is deliberate. Login looks a user up by email without knowing their type, and
    /// under table-per-type that query would otherwise LEFT JOIN all three derived tables. Carrying
    /// the discriminator on the base table lets the auth path project from <c>users</c> alone.
    /// It is set by each subtype's constructor, never by a caller.
    /// </remarks>
    public enum UserType
    {
        JobSeeker = 0,
        Employer = 1,
        SystemStaff = 2
    }
}
