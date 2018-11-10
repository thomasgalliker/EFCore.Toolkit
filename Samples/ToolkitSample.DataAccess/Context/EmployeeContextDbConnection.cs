using EFCore.Toolkit;

namespace ToolkitSample.DataAccess.Context
{
    /// <summary>
    /// This DbConnection implementation provides a ConnectionString for production.
    /// You can receive the production ConnectionString from an application configuration (app.config) if you like.
    /// </summary>
    public class EmployeeContextDbConnection : DbConnection
    {
        public EmployeeContextDbConnection()
            : base(name: "EntityFramework.Toolkit",
                   connectionString: $@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename={System.IO.Directory.GetCurrentDirectory()}\EntityFramework.Toolkit.mdf; Integrated Security=True;")
        {
            this.LazyLoadingEnabled = false;
            this.ProxyCreationEnabled = false;
        }
    }
}