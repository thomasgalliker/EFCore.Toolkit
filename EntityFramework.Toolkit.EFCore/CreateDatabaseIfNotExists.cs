using Microsoft.EntityFrameworkCore;

namespace EntityFramework.Toolkit.EFCore
{
    public class CreateDatabaseIfNotExists<T> : IDatabaseInitializer<T>
    {
        public void Initialize(DbContext context, bool force)
        {
            context.Database.EnsureCreated();
        }
    }
}