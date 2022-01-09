using System.Linq;
using EFCore.Toolkit.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit.Extensions
{
    public static class CreatedByExtensions
    {
        public static void ApplyCreatedBy<TKey>(this DbContext context, TKey createdBy)
        {
            var trackedEntries = context.ChangeTracker.Entries().ToList();
            foreach (var entry in trackedEntries)
            {
                if (entry.Entity is ICreatedBy<TKey> createdByEntity)
                {
                    if (entry.State == EntityState.Added && Equals(createdByEntity.CreatedBy, default(TKey)))
                    {
                        createdByEntity.CreatedBy = createdBy;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entry.Property(nameof(ICreatedBy<TKey>.CreatedBy)).IsModified = false;
                    }
                }
            }
        }
    }
}