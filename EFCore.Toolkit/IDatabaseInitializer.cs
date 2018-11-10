using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit
{
    public interface IDatabaseInitializer<T>
    {
        void Initialize(DbContext context, bool force);
    }
}