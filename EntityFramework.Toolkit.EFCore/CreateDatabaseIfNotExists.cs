using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFramework.Toolkit.EFCore.Testing
{
    public class CreateDatabaseIfNotExists<T> : IDatabaseInitializer<T>
    {
        public void Initialize(DatabaseFacade database, bool force)
        {
            database.EnsureCreated();
        }
    }
}