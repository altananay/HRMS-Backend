using Domain.Common;

namespace Domain.Entities
{
    /// <summary>
    /// A closed set of assignable roles, seeded at startup.
    /// </summary>
    /// <remarks>
    /// Replaces the denormalized <c>string[] Claims</c> array carried on each actor entity. That
    /// array was the privilege-escalation surface: <c>CreateSystemStaffCommand</c> exposed
    /// <c>Claims</c> on the request body and AutoMapper copied it straight onto the entity, so a
    /// caller could grant themselves "admin" simply by putting it in the JSON.
    ///
    /// A join table closes that in two ways — the set of roles is bounded by a foreign key, so
    /// "amdin" cannot be typo'd into existence, and roles can only be written through the explicit
    /// user_roles insert path, which lives in one admin-only code path.
    /// </remarks>
    public class Role : BaseEntity
    {
        public string Name { get; set; } = null!;

        public ICollection<UserRole> UserRoles { get; set; } = [];
    }
}
