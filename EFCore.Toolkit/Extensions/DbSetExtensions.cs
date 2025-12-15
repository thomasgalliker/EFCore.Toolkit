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

            var body = keySelector.Body;

            // Handle boxing: x => (object)x.Prop
            if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            {
                body = unary.Operand;
            }

            if (body is MemberExpression memberExpression)
            {
                // Single property selector: x => x.Id
                propertyInfos = new[] { (PropertyInfo)memberExpression.Member };
            }
            else if (body is NewExpression newExpression)
            {
                // Composite key selector: x => new { x.Id1, x.Id2 }
                var properties = new List<PropertyInfo>();

                foreach (var arg in newExpression.Arguments)
                {
                    if (arg is not MemberExpression m || m.Member is not PropertyInfo prop)
                    {
                        throw new InvalidOperationException(
                            "Composite key selector must consist of entity properties only.");
                    }

                    properties.Add(prop);
                }

                propertyInfos = properties.ToArray();
            }
            else if (body is NewArrayExpression newArrayExpression)
            {
                // Composite key selector: x => new object[] { x.Prop1, x.Prop2 }
                var properties = new List<PropertyInfo>();

                foreach (var expression in newArrayExpression.Expressions)
                {
                    var expr = expression;

                    while (expr is UnaryExpression u && u.NodeType == ExpressionType.Convert)
                    {
                        expr = u.Operand;
                    }

                    if (expr is not MemberExpression member ||
                        member.Member is not PropertyInfo prop)
                    {
                        throw new InvalidOperationException(
                            "Composite key selector must consist only of entity properties.");
                    }

                    properties.Add(prop);
                }

                propertyInfos = properties.ToArray();
            }
            else
            {
                throw new ArgumentException($"Invalid key selector expression: {body.GetType().Name}", nameof(keySelector));
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
