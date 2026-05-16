using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Utils;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit.Extensions
{
    public static class QueryableExtensions
    {
        private static readonly MethodInfo SoftDeleteAsyncMethod = typeof(QueryableExtensions).GetMethod(nameof(SoftDeleteAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

        /// <summary>
        /// Deletes all entities matching the given <paramref name="predicate"/> directly in the database
        /// using EF Core's <c>ExecuteDeleteAsync</c>. Entities are not loaded into the change tracker.
        /// <para>
        /// If <typeparamref name="TEntity"/> implements <see cref="IDeletable"/>, a soft delete is performed
        /// instead by issuing a single <c>UPDATE ... SET IsDeleted = 1</c> via <c>ExecuteUpdateAsync</c>.
        /// </para>
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="queryable">The source queryable (typically from <c>repository.Get()</c> or a <see cref="DbSet{TEntity}"/>).</param>
        /// <param name="predicate">A predicate selecting the entities to delete.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The number of rows affected on the server.</returns>
        public static Task<int> RemoveWhereAsync<TEntity>(
            [NotNull] this IQueryable<TEntity> queryable,
            [NotNull] Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
            where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(queryable);
            ArgumentNullException.ThrowIfNull(predicate);

            var queryableFiltered = queryable.Where(predicate);

            if (typeof(IDeletable).IsAssignableFrom(typeof(TEntity)))
            {
                return (Task<int>)SoftDeleteAsyncMethod
                    .MakeGenericMethod(typeof(TEntity))
                    .Invoke(null, new object[] { queryableFiltered, cancellationToken })!;
            }

            return queryableFiltered.ExecuteDeleteAsync(cancellationToken);
        }

        private static Task<int> SoftDeleteAsync<TEntity>(IQueryable<TEntity> queryable, CancellationToken cancellationToken)
            where TEntity : class, IDeletable
        {
            return queryable.ExecuteUpdateAsync(s => s.SetProperty(e => e.IsDeleted, true), cancellationToken);
        }

        /// <summary>
        /// Filters the elements of an System.Linq.IQueryable based on a specified <paramref name="type"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="queryable">An System.Linq.IQueryable whose elements to filter.</param>
        /// <param name="type">The type to filter the elements of the sequence on.</param>
        /// <returns>A collection that contains the elements from source that have <paramref name="type"/>.</returns>
        public static IQueryable<TEntity> OfType<TEntity>(this IQueryable<TEntity> queryable, Type type)
        {
            // TODO Check if type is subclass of T

            var ofTypeQueryable = (IQueryable<TEntity>)ReflectionHelper.InvokeGenericMethod(
                          null,
                          () => Queryable.OfType<object>(null!),
                          type,
                          new object[] { queryable })!;

            return ofTypeQueryable;
        }

        /// <summary>
        /// Includes one or more navigation properties specified by lambda expressions.
        /// Supports nested collection navigation using <c>Select</c>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type of the queryable.</typeparam>
        /// <param name="queryable">The source queryable.</param>
        /// <param name="navigationPropertyPaths">Lambda expressions specifying navigation properties to include.</param>
        /// <returns>A queryable with the specified navigation properties included.</returns>
        public static IQueryable<TEntity> IncludeNested<TEntity>([NotNull] this IQueryable<TEntity> queryable, [NotNull] params Expression<Func<TEntity, object?>>[] navigationPropertyPaths) where TEntity : class
        {
            if (queryable == null)
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            foreach (var navigationPropertyPath in navigationPropertyPaths)
            {
                queryable = IncludeNested(queryable, navigationPropertyPath);
            }

            return queryable;
        }

        /// <summary>
        /// Includes a single navigation property specified by a lambda expression.
        /// Supports nested collection navigation using <c>Select</c>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type of the queryable.</typeparam>
        /// <typeparam name="TProperty">The property type of the navigation property.</typeparam>
        /// <param name="queryable">The source queryable.</param>
        /// <param name="navigationPropertyPath">Lambda expression specifying the navigation property.</param>
        /// <returns>A queryable with the specified navigation property included.</returns>
        public static IQueryable<TEntity> IncludeNested<TEntity, TProperty>([NotNull] this IQueryable<TEntity> queryable, [NotNull] Expression<Func<TEntity, TProperty?>> navigationPropertyPath) where TEntity : class
        {
            if (queryable == null)
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            if (navigationPropertyPath == null)
            {
                throw new ArgumentNullException(nameof(navigationPropertyPath));
            }

            if (!DbHelpers.TryParsePath(navigationPropertyPath.Body, out var path) || path == null)
            {
                throw new ArgumentException(
                    "A specified Include path is not valid. The given path expression may contains invalid elements.",
                    nameof(navigationPropertyPath));
            }

            return queryable.Include(path);
        }
    }
}