using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCore.Toolkit.Extensions
{
    public static class DbContextExtensions
    {
        public static bool AllMigrationsApplied(this DbContext context)
        {
            var applied = context.GetService<IHistoryRepository>()
                .GetAppliedMigrations()
                .Select(m => m.MigrationId);

            var total = context.GetService<IMigrationsAssembly>()
                .Migrations
                .Select(m => m.Key);

            return !total.Except(applied).Any();
        }

        public static void Seed(this DbContext context, IEnumerable<IDataSeed> dataSeeds)
        {
            foreach (var dataSeed in dataSeeds)
            {
                var predicate = dataSeed.GetAddOrUpdateExpression();

                ReflectionHelper.InvokeGenericMethod(
                    null,
                    () => DbContextExtensions.AddOrUpdate<object>(null, null, null),
                    dataSeed.EntityType,
                    new object[] { context, predicate, dataSeed.GetAllObjects() });
            }
        }

        public static void AddOrUpdate<TEntity>(this DbContext context, Expression<Func<object, object>> propertyExpression, params object[] entities) where TEntity : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            var set = context.Set<TEntity>();
            var parameter = Expression.Parameter(typeof(TEntity));
            var propertyName = propertyExpression.GetPropertyInfo().Name;
            var property = Expression.Property(parameter, propertyName);
            foreach (var entity in entities)
            {
                var propertyValue = entity.GetPropertyValue(propertyName);
                var equalExpression = Expression.Equal(property, Expression.Constant(propertyValue));
                var lambdaExpression = Expression.Lambda<Func<TEntity, bool>>(equalExpression, parameter);
                var existingEntity = set.SingleOrDefault(lambdaExpression);
                if (existingEntity != null)
                {
                    context.Entry(existingEntity).CurrentValues.SetValues(entity);
                }
                else
                {
                    context.Entry(entity).State = EntityState.Added;
                }
            }
        }

        /// <summary>
        ///     Adds an entity (if newly created) or update (if has non-default Id).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The db context.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <remarks>
        ///     Will not work for HasDatabaseGeneratedOption(DatabaseGeneratedOption.None).
        ///     Will not work for composite keys.
        /// </remarks>
        public static T AddOrUpdate<T>(this DbContext context, T entity) where T : class
        {
            // Source: https://stackoverflow.com/questions/36208580/what-happened-to-addorupdate-in-ef-7-core

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var entityEntry = context.Entry(entity);

            var primaryKeyName = entityEntry.Context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties
                .Select(x => x.Name).Single();

            var primaryKeyField = entity.GetType().GetRuntimeProperty(primaryKeyName);

            var t = typeof(T);
            if (primaryKeyField == null)
            {
                throw new Exception($"{t.FullName} does not have a primary key specified. Unable to exec AddOrUpdate call.");
            }
            var keyVal = primaryKeyField.GetValue(entity);
            var dbVal = context.Set<T>().Find(keyVal);

            if (dbVal != null)
            {
                context.Entry(dbVal).CurrentValues.SetValues(entity);
                context.Set<T>().Update(dbVal);

                entity = dbVal;
            }
            else
            {
                context.Set<T>().Add(entity);
            }

            return entity;
        }

#if !NETSTANDARD1_3 && !NETFX
        /// <summary>
        /// Returns the number of table rows per database table.
        /// </summary>
        public static List<TableRowCounts> GetTableRowCounts(this DbContext c)
        {
            var rawSqlQuery = c.Query<TableRowCounts>().FromSql(
                @"CREATE TABLE #counts
                    (
                        TableName varchar(255),
                        TableRowCount int
                    )

                    EXEC sp_MSForEachTable @command1='INSERT #counts (TableName, TableRowCount) SELECT ''?'', COUNT(*) FROM ?'
                    SELECT TableName, TableRowCount FROM #counts ORDER BY TableName, TableRowCount DESC
                    DROP TABLE #counts");

            var tableCountResults = rawSqlQuery.ToList();
            return tableCountResults;
        }
#endif

        public static IQueryable Set(this DbContext context, Type entityType)
        {
            // Get the generic type definition
            MethodInfo method = typeof(DbContext).GetRuntimeMethod(nameof(DbContext.Set), new Type[] { });

            // Build a method with the specific type argument you're interested in
            method = method.MakeGenericMethod(entityType);

            return method.Invoke(context, null) as IQueryable;
        }

        public static IQueryable<T> Set<T>(this DbContext context)
        {
            // Get the generic type definition 
            MethodInfo method = typeof(DbContext).GetRuntimeMethod(nameof(DbContext.Set), null);

            // Build a method with the specific type argument you're interested in 
            method = method.MakeGenericMethod(typeof(T));

            return method.Invoke(context, null) as IQueryable<T>;
        }
    }
}