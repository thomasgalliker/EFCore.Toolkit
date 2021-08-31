using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ToolkitSample.DataAccess.Context;

namespace ToolkitSample.DataAccess
{

    public class DesignTimeEmployeeContextFactory : IDesignTimeDbContextFactory<EmployeeContext>
    {
        public EmployeeContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<EmployeeContext>();
            var dbContextOptions = builder
                .UseSqlServer("Data Source=(local);Initial Catalog=Activities;Integrated Security=False;User ID=user;Password=pw;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;")
                .Options;

            var context = new EmployeeContext(new EmployeeContextDbConnection()
                , s => Debug.WriteLine(s));

            return context;
        }
    }
}
