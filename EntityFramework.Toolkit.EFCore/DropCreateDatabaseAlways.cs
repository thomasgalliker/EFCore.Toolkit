using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFramework.Toolkit.EFCore.Testing
{
    public class DropCreateDatabaseAlways<T> : IDatabaseInitializer<T>
    {
        public void Initialize(DatabaseFacade database, bool force)
        {
            database.EnsureDeleted();
            database.EnsureCreated();
        }
    }
}