using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EFCore.Toolkit.Abstractions.Extensions
{
    public static class ReadOnlyRepositoryExtensions
    {  /// <summary>
       ///     Finds entities with the given <paramref name="predicate"/>.
       /// </summary>
       /// <param name="predicate">The search predicate.</param>
       /// <returns>A collection of entities matching the search predicate.</returns>
        public static IEnumerable<T> FindBy<T>(this IReadOnlyRepository<T> repository, Expression<Func<T, bool>> predicate)
        {
            IEnumerable<T> query = repository.Get().Where(predicate).AsEnumerable();
            return query;
        }

        /// <summary>
        ///     Indicates whether an entity with the given primary key value exists.
        /// </summary>
        /// <param name="ids">The primary keys of the entity to be found.</param>
        /// <returns>true, if an entity with given primary key exists; otherwise, false.</returns>
        public static bool Any<T>(this IReadOnlyRepository<T> repository, params object[] ids)
        {
            return repository.FindById(ids) != null;
        }

        /// <summary>
        ///     Indicates whether an entity which matches the given predicate exists.
        /// </summary>
        /// <param name="predicate">The predicate to filter the entity.</param>
        /// <returns>true, if an entity exists for given predicate; otherwise, false.</returns>
        public static bool Any<T>(this IReadOnlyRepository<T> repository, Func<T, bool> predicate)
        {
            return repository.Get().Any(predicate);
        }
    }
}