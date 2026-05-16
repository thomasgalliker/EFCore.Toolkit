using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using EFCore.Toolkit.Abstractions;

namespace EFCore.Toolkit.Extensions
{
    public static class GenericRepositoryExtensions
    {
        /// <summary>
        /// Deletes all entities in the <paramref name="repository"/> matching the given <paramref name="predicate"/>
        /// directly in the database. Entities are not loaded into the change tracker and there is no need to call
        /// <see cref="IWritableRepository.SaveAsync(CancellationToken)"/> afterwards.
        /// <para>
        /// If <typeparamref name="TEntity"/> implements <see cref="IDeletable"/>, a soft delete is performed
        /// (<c>UPDATE ... SET IsDeleted = 1</c>) instead of a physical delete.
        /// </para>
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

            return repository.Get().RemoveWhereAsync(predicate, cancellationToken);
        }
    }
}
