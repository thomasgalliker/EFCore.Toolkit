namespace EFCore.Toolkit.Abstractions.Extensions
{
    public static class IdentifiableExtensions
    {
        public static IEnumerable<T> ResetIds<T>(this IEnumerable<T> source) where T : IIdentifiable
        {
            foreach (var item in source)
            {
                item.Id = 0;
                yield return item;
            }
        }

        public static IEnumerable<T> ResetExternalIds<T>(this IEnumerable<T> source) where T : IExternalIdentifiable
        {
            foreach (var item in source)
            {
                item.ExternalId = Guid.Empty;
                yield return item;
            }
        }

        public static IEnumerable<T> ResetAllIds<T>(this IEnumerable<T> source) where T : IIdentifiable, IExternalIdentifiable
        {
            foreach (var item in source)
            {
                item.Id = 0;
                item.ExternalId = Guid.Empty;
                yield return item;
            }
        }
    }
}