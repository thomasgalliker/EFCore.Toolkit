namespace EFCore.Toolkit.Abstractions.Extensions
{
    public static class GenericRepositoryExtensions
    {
        /// <summary>
        /// Marks the entity with the specified primary key as <c>Deleted</c>, 
        /// so that it will be removed from the database when <see cref="IGenericRepository{TEntity}.SaveChanges"/> is called.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to remove.</typeparam>
        /// <param name="repository">The repository.</param>
        /// <param name="ids">The primary key values of the entity to remove.</param>
        /// <returns>The entity that has been removed.</returns>
        /// <remarks>
        /// If the entity exists in the context with an <c>Added</c> state, it will be detached rather than deleted, 
        /// because an entity not yet persisted in the database cannot be deleted.
        /// </remarks>
        public static TEntity RemoveById<TEntity>(this IGenericRepository<TEntity> repository, params object[] ids)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            var entityToRemove = repository.FindById(ids);
            if (entityToRemove == null)
            {
                throw new ArgumentException($"RemoveById could not find entity with ID=[{string.Join(",", ids)}]", nameof(ids));
            }

            return repository.Remove(entityToRemove);
        }

        /// <summary>
        /// Removes the entity with the specified <paramref name="id"/> from the repository.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity, which must implement <see cref="IIdentifiable"/>.</typeparam>
        /// <param name="repository">The repository.</param>
        /// <param name="id">The integer identifier of the entity to remove.</param>
        /// <returns>The entity that has been removed.</returns>
        public static TEntity RemoveById<TEntity>(this IGenericRepository<TEntity> repository, int id) where TEntity : IIdentifiable
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            var entityToRemove = repository.Get().FindById(id);
            if (entityToRemove == null)
            {
                throw new ArgumentException($"RemoveById could not find entity with id={id}", nameof(id));
            }

            return repository.Remove(entityToRemove);
        }

        /// <summary>
        /// Removes the entity with the specified <paramref name="externalId"/> from the repository.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity, which must implement <see cref="IExternalIdentifiable"/>.</typeparam>
        /// <param name="repository">The repository.</param>
        /// <param name="externalId">The external <see cref="Guid"/> identifier of the entity to remove.</param>
        /// <returns>The entity that has been removed.</returns>
        public static TEntity RemoveByExternalId<TEntity>(this IGenericRepository<TEntity> repository, Guid externalId) where TEntity : IExternalIdentifiable
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            var entityToRemove = repository.Get().FindByExternalId(externalId);
            if (entityToRemove == null)
            {
                throw new ArgumentException($"RemoveByExternalId could not find entity with externalId={externalId}", nameof(externalId));
            }

            return repository.Remove(entityToRemove);
        }

        /// <summary>
        /// Removes all entities from the repository that match the conditions defined by the given <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities to remove.</typeparam>
        /// <param name="repository">The repository.</param>
        /// <param name="predicate">
        /// A function to test each entity for a condition. If <c>null</c>, all entities in the repository will be removed.
        /// </param>
        /// <returns>An <see cref="IEnumerable{TEntity}"/> of entities that were removed.</returns>
        /// <remarks>
        /// This method retrieves entities from the repository before removal. 
        /// Entities are only removed from the repository when <see cref="IGenericRepository{TEntity}.RemoveRange"/> is called internally.
        /// </remarks>
        public static IEnumerable<TEntity> RemoveAll<TEntity>(this IGenericRepository<TEntity> repository, Func<TEntity, bool>? predicate = null)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            IEnumerable<TEntity> query = repository.Get();
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return repository.RemoveRange(query);
        }
    }
}