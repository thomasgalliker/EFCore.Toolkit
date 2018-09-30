using Microsoft.EntityFrameworkCore;

namespace EntityFramework.Toolkit.EFCore
{
    public class DropCreateDatabaseAlways<T> : IDatabaseInitializer<T>
    {
        public void Initialize(DbContext context, bool force)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}