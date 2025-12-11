namespace EFCore.Toolkit.Abstractions.Extensions
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Filters entities which implement <seealso cref="ICreatedBy{TKey}"/> and match the specified
        /// <paramref name="createdBy"/> value with <c>CreatedBy</c> property of <c>ICreatedBy</c>.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <typeparam name="TKey">Type of user ID which relates to the entity.</typeparam>
        /// <param name="queryable">The queryable to be filtered.</param>
        /// <param name="createdBy">The user ID for which queryable is filtered.</param>
        /// <returns>Queryable which contains only those entities which belong to user with ID <paramref name="createdBy"/>.</returns>
        public static IQueryable<T> WhereCreatedBy<T, TKey>(this IQueryable<T> queryable, TKey createdBy) where T : class, ICreatedBy<TKey>
        {
            return queryable.Where(i => Equals(i.CreatedBy, createdBy));
        }

        public static int? FindIdByExternalId<TEntity>(this IQueryable<TEntity> queryable, Guid externalId) where TEntity : IExternalIdentifiable, IIdentifiable
        {
            var entity = queryable
                .Select(x => new { x.Id, x.ExternalId })
                .SingleOrDefault(i => i.ExternalId == externalId);

            if (entity == null)
            {
                return null;
            }

            return entity.Id;
        }

        public static TEntity FindByExternalId<TEntity>(this IQueryable<TEntity> queryable, Guid externalId) where TEntity : IExternalIdentifiable
        {
            return queryable.SingleOrDefault(i => i.ExternalId == externalId);
        }

        public static int GetNextId<TEntity>(this ICollection<TEntity> items) where TEntity : IIdentifiable
        {
            if (items.Any())
            {
                var lastId = items.Max(t => t.Id);
                var nextId = lastId + 1;
                return nextId;
            }

            return 1;
        }
    }
}
