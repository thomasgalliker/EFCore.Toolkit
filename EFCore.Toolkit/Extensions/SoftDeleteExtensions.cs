using System.Linq;
using EFCore.Toolkit.Abstractions;

namespace EFCore.Toolkit.Extensions
{
    public static class SoftDeleteExtensions
    {
        public static IQueryable<T> FilterDeleted<T>(this IQueryable<T> repository, bool isDeleted = true) where T : IDeletable
        {
            return repository.Where(x => x.IsDeleted == isDeleted);
        }
    }
}