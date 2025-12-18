namespace EFCore.Toolkit.Extensions
{
    internal static class AsyncEnumerableExtensions
    {
        internal static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable, CancellationToken cancellationToken = default)
        {
            var results = new List<T>();
            await foreach (var item in asyncEnumerable.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                results.Add(item);
            }

            return results;
        }
    }
}