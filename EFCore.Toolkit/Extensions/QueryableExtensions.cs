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
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="queryable">An System.Linq.IQueryable whose elements to filter.</param>
        /// <param name="type">The type to filter the elements of the sequence on.</param>
        /// <returns>A collection that contains the elements from source that have <paramref name="type"/>.</returns>
        public static IQueryable<T> OfType<T>(this IQueryable<T> queryable, Type type)
        {
            // TODO Check if type is subclass of T

            var ofTypeQueryable = (IQueryable<T>)ReflectionHelper.InvokeGenericMethod(
                          null,
                          () => Queryable.OfType<object>(null),
                          type,
                          new object[] { queryable });

            return ofTypeQueryable;
        }

        [Obsolete("Use Include method instead.")]
        public static IQueryable<T> Include2<T>(this IQueryable<T> queryable, params Expression<Func<T, object>>[] properties) where T : class
        {
            if (queryable == null)
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            foreach (var property in properties)
            {
                queryable = QueryableExtensions.Include2<T, object>(queryable, property);
            }

            return queryable;
        }

        [Obsolete("Use Include method instead.")]
        public static IQueryable<T> Include2<T, TProperty>(this IQueryable<T> queryable, Expression<Func<T, TProperty>> pathExpression) where T : class
        {
            if (queryable == null)
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            if (pathExpression == null)
            {
                throw new ArgumentNullException(nameof(pathExpression));
            }

            if (!DbHelpers.TryParsePath(pathExpression.Body, out var path) || path == null)
            {
                throw new ArgumentException("A specified Include path is not valid. The given path expression may contains invalid elements.", nameof(pathExpression));
            }

            return queryable.Include(path);
        }
    }
}