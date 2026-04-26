using EFCore.Toolkit.Abstractions;

namespace EFCore.Toolkit.Extensions
{
    public static class EnumerableExtensions
    {
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