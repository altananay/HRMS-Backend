namespace Domain.Common
{
    /// <summary>
    /// Marks an entity that is hidden rather than physically removed.
    /// </summary>
    /// <remarks>
    /// Applied deliberately narrowly — only <c>User</c> (and its TPT subtypes) and
    /// <c>JobAdvertisement</c>. Those are the two cases where a hard delete destroys someone else's
    /// record: removing a job seeker today orphans their applications, and removing an advertisement
    /// erases the employer's hiring history.
    ///
    /// It is not applied globally on purpose. A blanket soft delete forces every unique index to
    /// become partial and silently breaks cascade deletes, which trades one class of bug for a
    /// subtler one. Contacts, CV files and job positions hard-delete perfectly well.
    /// </remarks>
    public interface ISoftDeletable
    {
        DateTime? DeletedAt { get; set; }
    }
}
