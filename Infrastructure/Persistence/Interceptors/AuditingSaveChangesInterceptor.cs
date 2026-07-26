using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Persistence.Interceptors
{
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
                        // Without this an update rewrites CreatedAt to the time it was last touched.
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

                entry.State = EntityState.Modified;
                entry.Entity.DeletedAt = utcNow;
            }
        }
    }
}
