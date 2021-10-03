using System;
using System.Collections.Generic;
using System.Linq;

namespace EFCore.Toolkit.Abstractions.Extensions
{
    public static class IdentifiableExtensions
    {
        public static int? FindIdByExternalId<T>(this IQueryable<T> repository, Guid externalId) where T : IExternalIdentifiable, IIdentifiable
        {
            var id = repository.Select(x => new { x.Id, x.ExternalId }).SingleOrDefault(i => i.ExternalId == externalId)?.Id;
            return id;
        }

        public static T FindByExternalId<T>(this IQueryable<T> repository, Guid externalId) where T : IExternalIdentifiable
        {
            return repository.SingleOrDefault(i => i.ExternalId == externalId);
        }

        public static void RemoveByExternalId<T>(this IGenericRepository<T> repository, Guid externalId) where T : IExternalIdentifiable
        {
            repository.RemoveAll(i => i.ExternalId == externalId);
        }

        public static int GetNextId<T>(this IEnumerable<T> items) where T : IIdentifiable
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