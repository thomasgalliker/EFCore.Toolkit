using System.Linq;
using EFCore.Toolkit.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit.Extensions
{
    public static class CreatedByExtensions
    {
        public static void SetCreatedBy<TKey>(this DbContext context, TKey userId)
        {
            foreach (var entityEntry in context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added))
            {
                if (entityEntry.Entity is ICreatedBy<TKey> entityToMark)
                {
                    entityToMark.CreatedBy = userId;
                }
            }
        }
    }

}