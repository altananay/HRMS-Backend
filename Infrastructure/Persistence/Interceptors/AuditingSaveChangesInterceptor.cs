using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Persistence.Interceptors
{
    /// <summary>
    /// Stamps <c>CreatedAt</c>/<c>UpdatedAt</c> and turns deletes of <see cref="ISoftDeletable"/>
    /// entities into updates.
    /// </summary>
    /// <remarks>
    /// Timestamps came from scattered <c>DateTime.UtcNow</c> — and in several managers
    /// <c>DateTime.Now</c> — assignments hand-written in every Add and Update method, which meant
    /// they were routinely forgotten or set inconsistently.
    ///
    /// Time comes from an injected <see cref="TimeProvider"/> rather than the static clock, so tests
    /// can advance it deterministically instead of sleeping.
    /// </remarks>
    public sealed class AuditingSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly TimeProvider _timeProvider;

        public AuditingSaveChangesInterceptor(TimeProvider timeProvider) => _timeProvider = timeProvider;

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            Apply(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            Apply(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void Apply(DbContext? context)
        {
            if (context is null)
            {
                return;
            }

            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = utcNow;
                        entry.Entity.UpdatedAt = null;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = utcNow;
                        // Never let an update rewrite the creation timestamp.
                        entry.Property(entity => entity.CreatedAt).IsModified = false;
                        break;
                }
            }

            foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
            {
                if (entry.State != EntityState.Deleted)
                {
                    continue;
                }

                // Rewrite the delete as an update so dependent rows (applications on an
                // advertisement, a seeker's history) survive.
                entry.State = EntityState.Modified;
                entry.Entity.DeletedAt = utcNow;
            }
        }
    }
}
