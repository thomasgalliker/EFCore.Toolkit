using System.Linq.Expressions;
using System.Reflection;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Abstractions.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.Toolkit.Extensions
{
    public static class DbSetExtensions
    {
        public static TEntity? AddOrUpdate<TEntity>(this DbSet<TEntity> dbSet, TEntity entity) where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(entity);

            var entities = new[] { entity };
            return AddOrUpdateInternal(dbSet.GetContext(), dbSet, entities, keySelector: null).SingleOrDefault();
        }

        public static TEntity? AddOrUpdate<TEntity>(this DbSet<TEntity> dbSet, TEntity entity, Expression<Func<TEntity, object?>> keySelector) where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(entity);

            var entities = new[] { entity };
            return AddOrUpdateInternal(dbSet.GetContext(), dbSet, entities, keySelector).SingleOrDefault();
        }

        public static TEntity[] AddOrUpdate<TEntity>(this DbSet<TEntity> dbSet, TEntity[] entities) where TEntity : class
        {
            return AddOrUpdateInternal(dbSet.GetContext(), dbSet, entities, null);
        }

        public static TEntity[] AddOrUpdate<TEntity>(this DbSet<TEntity> dbSet, TEntity[] entities, Expression<Func<TEntity, object?>>? keySelector) where TEntity : class
        {
            return AddOrUpdateInternal(dbSet.GetContext(), dbSet, entities, keySelector);
        }

        internal static TEntity[] AddOrUpdateInternal<TEntity>(DbContext context, DbSet<TEntity> dbSet, TEntity[] entities, Expression<Func<TEntity, object?>>? keySelector) where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(dbSet);
            ArgumentNullException.ThrowIfNull(entities);

            if (entities.Length == 0)
            {
                return entities;
            }

            var entityType = context.Model.FindEntityType(typeof(TEntity))
                ?? throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} not found.");

            var concurrencyTokenProperties = entityType.GetProperties()
                  .Where(p => p.IsConcurrencyToken)
                  .ToArray();

            var primarykeys = GetPrimaryKeyProperties(typeof(TEntity), entityType);

            var keys = keySelector != null
                ? ExtractKeyProperties(keySelector)
                : primarykeys;

            var existingEntities = dbSet.ToList()
                .Select(e => (Entity: e, KeyValues: keys.Select(k => (Key: k, Value: k.GetValue(e)))))
                .ToArray();

            var updatedEntities = new List<TEntity>(entities.Length);

            foreach (var entity in entities)
            {
                var existingEntity = existingEntities
                    .FirstOrDefault(e => e.KeyValues.All(kv => kv.Value?.Equals(kv.Key.GetValue(entity)) == true)).Entity;

                if (existingEntity == null)
                {
                    dbSet.Add(entity);
                    updatedEntities.Add(entity);
                    continue;
                }

                if (existingEntity is IDeletable deletable && deletable.IsDeleted)
                {
                    continue;
                }

                var entry = context.Entry(existingEntity);
                RestorePrimaryKeys(entity, existingEntity, primarykeys);
                entry.CurrentValues.SetValues(entity);
                RestoreConcurrencyTokens(entry, concurrencyTokenProperties);
                entry.State = EntityState.Modified;
                updatedEntities.Add(existingEntity);
            }

            return updatedEntities.ToArray();
        }


        private static void RestorePrimaryKeys(object entity, object existingEntity, PropertyInfo[] primaryKeys)
        {
            foreach (var primaryKey in primaryKeys)
            {
                var value = primaryKey.GetValue(existingEntity);
                primaryKey.SetValue(entity, value);
            }
        }

        private static void RestoreConcurrencyTokens<TEntity>(EntityEntry<TEntity> entry, IProperty[] properties) where TEntity : class
        {
            foreach (var property in properties)
            {
                entry.CurrentValues[property.Name] = entry.OriginalValues[property.Name];
            }
        }

        private static PropertyInfo[] ExtractKeyProperties<TEntity>(Expression<Func<TEntity, object?>> keySelector)
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

        private static PropertyInfo[] GetPrimaryKeyProperties(Type type, IEntityType entityType)
        {
            return entityType.FindPrimaryKey()?
                .Properties
                .Select(p => type.GetProperty(p.Name)!)
                .ToArray() ?? Array.Empty<PropertyInfo>();
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