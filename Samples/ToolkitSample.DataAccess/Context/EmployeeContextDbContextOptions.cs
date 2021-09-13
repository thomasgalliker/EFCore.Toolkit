using EFCore.Toolkit.Testing;
using Microsoft.EntityFrameworkCore;
using ToolkitSample.DataAccess.Context;

namespace ToolkitSample.DataAccess
{
    public class EmployeeContextDbContextOptions
    {
        public static DbContextOptions Create<TContext>() where TContext : DbContext
        {
            var connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=ToolkitSa;Trusted_Connection=True;MultipleActiveResultSets=true;".RandomizeDatabaseName();

            var dbContextOptions = new DbContextOptionsBuilder<TContext>()
                .UseSqlServer(connectionString)
                .Options;

            return dbContextOptions;
        }
    }
}