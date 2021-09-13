using System.Linq;

namespace EFCore.Toolkit.Abstractions
{
    public interface IUserContextAwareRepository<T>
    {
        IQueryable<T> Get(bool filterByCurrentUser);
    }
}