using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Persistence.ValueGeneration
{
    /// <summary>
    /// Generates a version 7 GUID for a primary key, at the moment the entity is saved.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Version 7 rather than EF's default version 4: a v7 GUID is time-ordered, so consecutive inserts
    /// land next to each other in the index instead of scattering across it. That was the reason the
    /// codebase generated keys itself in the first place, and it is worth keeping.
    /// </para>
    /// <para>
    /// It is generated <em>here</em>, and not in <c>BaseEntity</c>'s property initializer, because a
    /// pre-filled key is indistinguishable from an existing row. EF decides whether an entity it
    /// discovers inside a tracked parent's collection is new or already persisted by asking whether
    /// its key is set — so a new <c>Education</c> added to a loaded <c>Cv</c> was tracked as
    /// <c>Modified</c> and saved with an <c>UPDATE</c> that matched no rows, throwing
    /// <c>DbUpdateConcurrencyException</c>. No job seeker could add an education, a job, a language or
    /// a project to a saved CV, and the same shape would have broken employer departments.
    /// </para>
    /// <para>
    /// <c>GeneratesTemporaryValues</c> is <c>false</c>: the value is the real key, written as-is.
    /// </para>
    /// </remarks>
    public sealed class GuidV7ValueGenerator : ValueGenerator<Guid>
    {
        public override bool GeneratesTemporaryValues => false;

        public override Guid Next(EntityEntry entry) => Guid.CreateVersion7();
    }
}
