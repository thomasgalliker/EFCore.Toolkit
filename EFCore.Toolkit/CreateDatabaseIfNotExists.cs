using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit
{
    public class CreateDatabaseIfNotExists<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
    {
        public void Initialize(DbContextBase<TContext> context, bool force)
        {
            context.Database.EnsureCreated();
        }
    }
}