using System.Reflection;
using EFCore.Toolkit.Utils;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit.Extensions
{
    public static class DbSetExtensions
    {
        public static void AddOrUpdate<TEntity>(this DbSet<TEntity> dbSet, TEntity entity) where TEntity : class
        {
            dbSet.AddOrUpdate(entity);
        }

        public static void AddOrUpdate<TEntity>(this DbSet<TEntity> dbSet, params TEntity[] newEntities) where TEntity : class
        {
            var context = dbSet.GetContext();
            var primaryKeys = context.Model.FindEntityType(typeof(TEntity))
                .FindPrimaryKey()
                .Properties
                .Select(x => x.Name)
                .ToList();

            var entityType = typeof(TEntity);
            var primaryKeyPropertyInfos = new List<PropertyInfo>();

            foreach (var propertyInfo in entityType.GetProperties())
            {
                var isPrimaryKey = primaryKeys.Contains(propertyInfo.Name);
                if (isPrimaryKey)
                {
                    primaryKeyPropertyInfos.Add(propertyInfo);
                }
            }

            if (primaryKeyPropertyInfos.Count <= 0)
            {
                throw new InvalidOperationException(
                    $"{entityType.Name} does not have a KeyAttribute field");
            }

            var entities = dbSet.AsNoTracking().IgnoreQueryFilters();

            foreach (var entity in newEntities)
            {
                var filteredEntities = entities.ToArray();

                foreach (var primaryKeyPropertyInfo in primaryKeyPropertyInfos)
                {
                    var primaryKey = primaryKeyPropertyInfo.GetValue(entity);
                    filteredEntities = filteredEntities
                        .Where(e => e.GetType().GetProperty(primaryKeyPropertyInfo.Name).GetValue(e).Equals(primaryKey))
                        .ToArray();
                }

                var existingEntity = filteredEntities.FirstOrDefault();
                if (existingEntity == null || !IsDeleted(existingEntity))
                {
                    if (existingEntity != null)
                    {
                        context.Entry(existingEntity).CurrentValues.SetValues(entity);
                        context.Entry(existingEntity).State = EntityState.Modified;
                    }
                    else
                    {
                        dbSet.Add(entity);
                    }
                }
            }
        }

        private static bool IsDeleted<TEntity>(TEntity entity)
        {
            var entityType = typeof(TEntity);
            var propertyInfo = entityType.GetProperties().FirstOrDefault(c => c.Name == "Deleted" || c.Name == "IsDeleted");
            if (propertyInfo == null)
            {
                return false;
            }

            return (bool)propertyInfo.GetValue(entity);
        }

        internal static DbContext GetContext<TEntity>(this DbSet<TEntity> dbSet)
          where TEntity : class
        {
            return (DbContext)dbSet
                .GetType().GetTypeInfo()
                .GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(dbSet);
        }
    }
}
