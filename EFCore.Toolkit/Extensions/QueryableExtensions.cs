using System;
using System.Linq;
using System.Linq.Expressions;
using EFCore.Toolkit.Utils;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit.Extensions
{
    public static class QueryableExtensions
    {
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
        /// <summary>
        ///     Includes navigation properties.
        /// </summary>
        /// <typeparam name="T">Generic type T.</typeparam>
        /// <param name="queryable">Queryable</param>
        /// <param name="properties">A list of navigation properties to include.</param>
        /// <returns>New queryable which includes the given navigation properties.</returns>
        public static IQueryable<T> Include<T>(this IQueryable<T> queryable, params Expression<Func<T, object>>[] properties) where T : class
        {
            if (queryable == null)
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            foreach (var property in properties)
            {
                queryable = QueryableExtensions.Include<T, object>(queryable, property);
            }

            return queryable;
        }

        /// <summary>
        ///     Includes navigation properties.
        /// </summary>
        /// <typeparam name="T">Generic type T.</typeparam>
        /// <typeparam name="TProperty">Generic property type TProperty.</typeparam>
        /// <param name="queryable">Queryable</param>
        /// <param name="pathExpression">The navigation property to include.</param>
        /// <returns>New queryable which includes the given navigation properties.</returns>
        public static IQueryable<T> Include<T, TProperty>(this IQueryable<T> queryable, Expression<Func<T, TProperty>> pathExpression) where T : class
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