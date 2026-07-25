using Domain.Common;

namespace Domain.Entities
{
    /// <summary>
    /// A shared lookup of position titles, unique by name.
    /// </summary>
    /// <remarks>
    /// This is a behaviour change. <c>JobAdvertisementManager.Add</c> created a brand-new
    /// JobPosition row for every advertisement and <c>Delete</c> removed it alongside the ad, so it
    /// behaved as a per-advertisement field wearing a lookup table's clothes. The existing admin
    /// endpoints (<c>POST/PUT/DELETE /api/JobPosition/*</c>) show a shared lookup was the intent.
    ///
    /// Consequences now that the name is unique: adding an advertisement resolves-or-creates the
    /// position, deleting an advertisement no longer deletes it, and the foreign key is
    /// <c>ON DELETE RESTRICT</c> so a position still in use cannot be removed.
    /// </remarks>
    public class JobPosition : BaseEntity
    {
        public string Name { get; set; } = null!;

        public ICollection<JobAdvertisement> JobAdvertisements { get; set; } = [];
    }
}
