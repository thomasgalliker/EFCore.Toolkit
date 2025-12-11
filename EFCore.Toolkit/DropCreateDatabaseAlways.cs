using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit
{
    public class DropCreateDatabaseAlways<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
    {
        public void Initialize(DbContextBase<TContext> context, bool force)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}