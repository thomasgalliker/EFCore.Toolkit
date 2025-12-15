using EFCore.Toolkit.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit
{
    public interface IDatabaseInitializer
    {
        void Initialize(DbContextBase context, bool force);
    }
}