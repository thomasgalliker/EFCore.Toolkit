using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using EFCore.Toolkit.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit.Extensions
{
    public static class GenericRepositoryExtensions
    {
        /// <summary>
        /// Deletes all entities in the <paramref name="repository"/> matching the given <paramref name="predicate"/>
        /// directly in the database. Entities are not loaded into the change tracker and there is no need to call
        /// <see cref="IWritableRepository.Save"/> afterwards.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="repository">The repository.</param>
        /// <param name="predicate">A predicate selecting the entities to delete.</param>
        /// <returns>The number of rows affected on the server.</returns>
        public static int RemoveWhere<TEntity>([NotNull] this IGenericRepository<TEntity> repository, [NotNull] Expression<Func<TEntity, bool>> predicate)
            where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(predicate);

            return repository.Get().Where(predicate).ExecuteDelete();
        }

        /// <summary>
        /// Deletes all entities in the <paramref name="repository"/> matching the given <paramref name="predicate"/>
        /// directly in the database. Entities are not loaded into the change tracker and there is no need to call
        /// <see cref="IWritableRepository.SaveAsync"/> afterwards.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="repository">The repository.</param>
        /// <param name="predicate">A predicate selecting the entities to delete.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The number of rows affected on the server.</returns>
        public static Task<int> RemoveWhereAsync<TEntity>([NotNull] this IGenericRepository<TEntity> repository, [NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(predicate);

            return repository.Get().Where(predicate).ExecuteDeleteAsync(cancellationToken);
        }
    }
}
