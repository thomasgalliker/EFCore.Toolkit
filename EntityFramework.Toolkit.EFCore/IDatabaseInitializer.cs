using Microsoft.EntityFrameworkCore;

namespace EntityFramework.Toolkit.EFCore
{
    public interface IDatabaseInitializer<T>
    {
        void Initialize(DbContext context, bool force);
    }
}