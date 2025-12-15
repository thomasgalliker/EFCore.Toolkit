using EFCore.Toolkit.Abstractions.Auditing;
using EFCore.Toolkit.Abstractions.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFCore.Toolkit.Interceptors
{
    public class UpdateAuditableInterceptor : SaveChangesInterceptor
    {
        private readonly DateTimeKind dateTimeKind;

        public UpdateAuditableInterceptor()
            : this(DateTimeKind.Utc)
        {
        }

        public UpdateAuditableInterceptor(DateTimeKind dateTimeKind)
        {
            this.dateTimeKind = dateTimeKind;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (eventData.Context is not null)
            {
                this.UpdateAuditableEntities(eventData.Context);
            }

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                this.UpdateAuditableEntities(eventData.Context);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditableEntities(DbContext context)
        {
            var date = DateTime.UtcNow.ToKind(this.dateTimeKind);
            var entities = context.ChangeTracker.Entries<IUpdatedDate>().ToList();

            foreach (var entry in entities)
            {
                //if (entry.Entity is ICreatedDate creatableEntity && entry.State == EntityState.Added)
                //{
                //    creatableEntity.CreatedDate = utcNow;
                //}
                //else if (entry.Entity is IUpdatedDate updatableEntity && entry.State == EntityState.Modified)
                //{
                //    updatableEntity.UpdatedDate = utcNow;
                //}


                var creatableEntity = entry.Entity as ICreatedDate;
                if (entry.State == EntityState.Added && creatableEntity != null)
                {
                    creatableEntity.CreatedDate = date;
                }

                if (entry.State == EntityState.Modified)
                {
                    if (creatableEntity != null)
                    {
                        entry.Property(nameof(ICreatedDate.CreatedDate)).IsModified = false;
                    }

                    if (entry.Entity is IUpdatedDate updateableEntity)
                    {
                        updateableEntity.UpdatedDate = date;
                    }
                }
            }
        }

    }
}