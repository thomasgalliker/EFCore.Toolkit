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
            var context = new EmployeeContext(EmployeeContextDbContextOptions.Create<EmployeeContext>(), s => Debug.WriteLine(s));
            return context;
        }
    }
}
