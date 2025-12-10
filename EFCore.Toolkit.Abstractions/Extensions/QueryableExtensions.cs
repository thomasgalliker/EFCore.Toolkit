namespace EFCore.Toolkit.Abstractions.Extensions
{
    public static class QueryableExtensions
    {
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
