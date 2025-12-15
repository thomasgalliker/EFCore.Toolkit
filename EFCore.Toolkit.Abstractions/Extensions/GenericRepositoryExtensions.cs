namespace EFCore.Toolkit.Abstractions.Extensions
{
    public static class GenericRepositoryExtensions
    {
        /// <summary>
        /// Marks the the entity with the given primary key as Deleted such that it will be deleted from the database when
        /// SaveChanges is called. Note that the entity must exist in the context in some other state before this method
        /// is called.
        /// </summary>
        /// <returns> The entity that has been removed.</returns>
        /// <remarks>
        /// Note that if the entity exists in the context in the Added state, then this method
        /// will cause it to be detached from the context.  This is because an Added entity is assumed not to
        /// exist in the database such that trying to delete it does not make sense.
        /// </remarks>
        public static TEntity RemoveById<TEntity>(this IGenericRepository<TEntity> repository, params object[] ids)
        {
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

        public static TEntity RemoveById<TEntity>(this IGenericRepository<TEntity> repository, int id) where TEntity : IIdentifiable
        {
            var entityToRemove = repository.Get().FindById(id);
            if (entityToRemove == null)
            {
                throw new ArgumentException($"RemoveByExternalId could not find entity with id={id}", nameof(id));
            }

            return repository.Remove(entityToRemove);
        }

        public static TEntity RemoveByExternalId<TEntity>(this IGenericRepository<TEntity> repository, Guid externalId) where TEntity : IExternalIdentifiable
        {
            var entityToRemove = repository.Get().FindByExternalId(externalId);
            if (entityToRemove == null)
            {
                throw new ArgumentException($"RemoveByExternalId could not find entity with externalId={externalId}", nameof(externalId));
            }

            return repository.Remove(entityToRemove);
        }

        /// <summary>Removes all entities that match the conditions defined by the given predicate.</summary>
        /// <returns>The removed entities.</returns>
        /// <param name="predicate">The expression that defines the conditions of the elements to remove.</param>
        public static IEnumerable<TEntity> RemoveAll<TEntity>(this IGenericRepository<TEntity> repository, Func<TEntity, bool>? predicate = null)
        {
            IEnumerable<TEntity> query = repository.Get();
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return repository.RemoveRange(query);
        }
    }
}