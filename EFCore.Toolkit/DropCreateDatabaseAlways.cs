using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit
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