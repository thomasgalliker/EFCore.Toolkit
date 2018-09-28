using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFramework.Toolkit.EFCore
{
    public interface IDatabaseInitializer<T>
    {
        void Initialize(DatabaseFacade database, bool force);
    }
}