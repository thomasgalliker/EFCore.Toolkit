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
                   connectionString: @"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\EF.Toolkit.Tests.mdf; Integrated Security=True;".RandomizeDatabaseName())
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Directory.GetCurrentDirectory());

            this.LazyLoadingEnabled = false;
            this.ProxyCreationEnabled = false;
        }
    }
}