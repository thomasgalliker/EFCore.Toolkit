using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit
{
    public class CreateDatabaseIfNotExists<T> : IDatabaseInitializer<T>
    {
        public void Initialize(DbContext context, bool force)
        {
            context.Database.EnsureCreated();
        }
    }
}