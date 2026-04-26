using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Abstractions.Auditing;
using EFCore.Toolkit.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFCore.Toolkit.Interceptors
{
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        private readonly DateTimeKind dateTimeKind;

        public SoftDeleteInterceptor()
            : this(DateTimeKind.Utc)
        {
        }

        public SoftDeleteInterceptor(DateTimeKind dateTimeKind)
        {
            this.dateTimeKind = dateTimeKind;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (eventData.Context is not null)
            {
                this.ApplySoftDelete(eventData.Context);
            }

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                this.ApplySoftDelete(eventData.Context);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ApplySoftDelete(DbContext context)
        {
            var date = DateTime.UtcNow.ToKind(this.dateTimeKind);
            var deletableEntries = context.ChangeTracker.Entries<IDeletable>().ToList();

            foreach (var entry in deletableEntries)
            {
                if (entry.Entity is IDeletable deletable && entry.State == EntityState.Deleted)
                {
                    deletable.IsDeleted = true;
                    entry.State = EntityState.Modified;

                    if (entry.Entity is IUpdatedDate updatable)
                    {
                        updatable.UpdatedDate = date;
                    }
                }
            }
        }
    }
}