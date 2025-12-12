using System.Linq.Expressions;
using System.Reflection;
using EFCore.Toolkit.Abstractions;
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

        public static TEntity[] AddOrUpdate<TEntity>(this DbContext context, params TEntity[] entities) where TEntity : class
        {
            return DbSetExtensions.AddOrUpdateInternal(context, context.Set<TEntity>(), entities, null);
        }

        public static TEntity[] AddOrUpdate<TEntity>(this DbContext context, TEntity[] entities, Expression<Func<TEntity, object>> keySelector) where TEntity : class
        {
            return DbSetExtensions.AddOrUpdateInternal(context, context.Set<TEntity>(), entities, keySelector);
        }

        public static TEntity[] AddOrUpdate<TEntity>(this IContext context, TEntity[] entities, Expression<Func<TEntity, object>> keySelector) where TEntity : class
        {
            var dbContext = (DbContext)context;
            return DbSetExtensions.AddOrUpdateInternal(dbContext, dbContext.Set<TEntity>(), entities, keySelector);
        }

        public static TEntity[] AddOrUpdate<TEntity>(this IDbContext context, TEntity[] entities) where TEntity : class
        {
            var dbContext = (DbContext)context;
            return DbSetExtensions.AddOrUpdateInternal(dbContext, dbContext.Set<TEntity>(), entities, null);
        }

        public static TEntity[] AddOrUpdate<TEntity>(this IDbContext context, TEntity[] entities, Expression<Func<TEntity, object>> keySelector) where TEntity : class
        {
            var dbContext = (DbContext)context;
            return DbSetExtensions.AddOrUpdateInternal(dbContext, dbContext.Set<TEntity>(), entities, keySelector);
        }

        /// <summary>
        /// Returns the number of table rows per database table.
        /// </summary>
        public static async Task<List<TableRowCounts>> GetTableRowCountsAsync<T>(this DbContextBase<T> c) where T : DbContext
        {
            var rawSqlQuery = c.ExecuteQuery<TableRowCounts>(
                @"CREATE TABLE #counts
                    (
                        TableName varchar(255),
                        TableRowCount int
                    )

                    EXEC sp_MSForEachTable @command1='INSERT #counts (TableName, TableRowCount) SELECT ''?'', COUNT(*) FROM ?'
                    SELECT TableName, TableRowCount FROM #counts ORDER BY TableName, TableRowCount DESC
                    DROP TABLE #counts");

            var tableCountResults = await rawSqlQuery.ToListAsync();
            return tableCountResults;
        }

        public static IQueryable Set(this DbContext context, Type entityType)
        {
            // Get the generic type definition
            MethodInfo method = typeof(DbContext).GetRuntimeMethod(nameof(DbContext.Set), new Type[] { });

            // Build a method with the specific type argument you're interested in
            method = method.MakeGenericMethod(entityType);

            return (IQueryable)method.Invoke(context, null);
        }

        public static IQueryable<T> Set<T>(this DbContext context)
        {
            // Get the generic type definition 
            MethodInfo method = typeof(DbContext).GetRuntimeMethod(nameof(DbContext.Set), null);

            // Build a method with the specific type argument you're interested in 
            method = method.MakeGenericMethod(typeof(T));

            return (IQueryable<T>)method.Invoke(context, null);
        }
    }
}
