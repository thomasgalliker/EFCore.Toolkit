using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using EFCore.Toolkit.Utils;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit.Extensions
{
    public static class QueryableExtensions
    {
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