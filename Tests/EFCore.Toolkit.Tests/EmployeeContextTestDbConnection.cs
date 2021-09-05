using EFCore.Toolkit.Testing;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit.Tests
{
    /// <summary>
    /// This DbConnection implementation provides a ConnectionString for testing purposes.
    /// </summary>
    public class EmployeeContextTestDbConnection<T> : DbContextOptionsBuilder<T> where T : DbContext
    {
        public EmployeeContextTestDbConnection()
        {
            var connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=EF.Toolkit.Tests;Trusted_Connection=True;MultipleActiveResultSets=true;".RandomizeDatabaseName();
            this.UseSqlServer(connectionString);
        }
    }

    public class EmployeeContextTestDbConnection
    {
        public static DbContextOptions CreateDbContextOptions<TContext>() where TContext : DbContext
        {
            var connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=EF.Toolkit.Tests;Trusted_Connection=True;MultipleActiveResultSets=true;".RandomizeDatabaseName();

            var dbContextOptions = new DbContextOptionsBuilder<TContext>()
                .UseSqlServer(connectionString)
                .Options;

            return dbContextOptions;
        }
    }


}