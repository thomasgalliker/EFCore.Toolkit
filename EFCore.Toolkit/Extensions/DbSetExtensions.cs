using System.Linq.Expressions;
using System.Reflection;
using EFCore.Toolkit.Abstractions.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit.Extensions
{
    public static class DbSetExtensions
    {
        public static TEntity? AddOrUpdate<TEntity>(this DbSet<TEntity> dbSet, TEntity entity) where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var entities = new[] { entity };
            return AddOrUpdateInternal(dbSet.GetContext(), dbSet, entities, keySelector: null).SingleOrDefault();
        }

        public static TEntity? AddOrUpdate<TEntity>(this DbSet<TEntity> dbSet, TEntity entity, Expression<Func<TEntity, object>> keySelector) where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var entities = new[] { entity };
            return AddOrUpdateInternal(dbSet.GetContext(), dbSet, entities, keySelector).SingleOrDefault();
        }

        public static TEntity[] AddOrUpdate<TEntity>(this DbSet<TEntity> dbSet, TEntity[] entities) where TEntity : class
        {
            return AddOrUpdateInternal(dbSet.GetContext(), dbSet, entities, null);
        }

        public static TEntity[] AddOrUpdate<TEntity>(this DbSet<TEntity> dbSet, TEntity[] entities, Expression<Func<TEntity, object>>? keySelector) where TEntity : class
        {
            return AddOrUpdateInternal(dbSet.GetContext(), dbSet, entities, keySelector);
        }

        internal static TEntity[] AddOrUpdateInternal<TEntity>(DbContext context, DbSet<TEntity> dbSet, TEntity[] entities, Expression<Func<TEntity, object>>? keySelector) where TEntity : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (dbSet == null)
            {
                throw new ArgumentNullException(nameof(dbSet));
            }

            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            entities = entities.Where(x => x != null).ToArray();

            if (entities.Length == 0)
            {
                return entities;
            }

            var keys = keySelector != null
                ? ExtractKeyProperties(keySelector)
                : GetPrimaryKeyProperties(context, typeof(TEntity));

            var existingEntities = dbSet.AsNoTracking().ToList();

            var updatedEntities = new List<TEntity>();

            foreach (var entity in entities)
            {
                // Try to find tracked entity first (avoids dual instance issues)
                var trackedEntity = context.ChangeTracker.Entries<TEntity>()
                    .Select(e => e.Entity)
                    .FirstOrDefault(e =>
                        keys.All(k =>
                            k.GetValue(e)?.Equals(k.GetValue(entity)) == true));

                // Fallback to untracked entities loaded earlier
                var existingEntity = trackedEntity ?? existingEntities
                    .FirstOrDefault(e =>
                        keys.All(k =>
                            k.GetValue(e)?.Equals(k.GetValue(entity)) == true));

                if (existingEntity == null)
                {
                    dbSet.Add(entity);
                    updatedEntities.Add(entity);
                }
                else if (!DeletableExtensions.IsDeleted(existingEntity))
                {
                    context.Entry(existingEntity).CurrentValues.SetValues(entity);
                    context.Entry(existingEntity).State = EntityState.Modified;
                    updatedEntities.Add(entity);
                }
            }

            return updatedEntities.ToArray();
        }

        private static PropertyInfo[] ExtractKeyProperties<TEntity>(Expression<Func<TEntity, object>> keySelector)
        {
            PropertyInfo[] propertyInfos;

            var body = keySelector.Body is UnaryExpression u ? u.Operand : keySelector.Body;

            if (body is MemberExpression member)
            {
                // Single property selector: x => x.Id
                propertyInfos = new[] { (PropertyInfo)member.Member };
            }
            else if (body is NewExpression anon)
            {
                // Composite key selector: x => new { x.Code, x.Type }
                propertyInfos = anon.Members.Cast<PropertyInfo>().ToArray();
            }
            else
            {
                throw new ArgumentException("Invalid key selector expression.", nameof(keySelector));
            }

            return propertyInfos;
        }

        private static PropertyInfo[] GetPrimaryKeyProperties(DbContext context, Type entityType)
        {
            return context.Model.FindEntityType(entityType)
                .FindPrimaryKey()
                .Properties
                .Select(p => entityType.GetProperty(p.Name))
                .ToArray();
        }

        internal static DbContext GetContext<TEntity>(this DbSet<TEntity> dbSet) where TEntity : class
        {
            if (dbSet == null)
            {
                throw new ArgumentNullException(nameof(dbSet));
            }

            var dbContext = dbSet.GetType()
                .GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(dbSet) as DbContext;

            return dbContext ?? throw new InvalidOperationException("Unable to retrieve DbContext from DbSet.");
        }
    }

}
