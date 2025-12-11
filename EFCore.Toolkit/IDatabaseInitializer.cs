using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit
{
    public interface IDatabaseInitializer<TContext> where TContext : DbContext
    {
        void Initialize(DbContextBase<TContext> context, bool force);
    }
}