using System;
using EFCore.Toolkit;
using EFCore.Toolkit.Testing;

namespace EFCore.Toolkit.Tests
{
    /// <summary>
    /// This DbConnection implementation provides a ConnectionString for testing purposes.
    /// </summary>
    public class EmployeeContextTestDbConnection : DbConnection
    {
        public EmployeeContextTestDbConnection()
            : base(name: "EFCore.Toolkit.Tests",
                   connectionString: @"Server=(localdb)\MSSQLLocalDB;Database=EF.Toolkit.Tests;Trusted_Connection=True;MultipleActiveResultSets=true;".RandomizeDatabaseName())
        {
            this.LazyLoadingEnabled = false;
        }
    }
}