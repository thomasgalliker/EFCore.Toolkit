using System.Linq;
using EntityFramework.Toolkit.EFCore;
using EntityFramework.Toolkit.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;
using ToolkitSample.DataAccess.Context;
using ToolkitSample.Model;

namespace ToolkitSample.DataAccess
{
    public class EmployeeContextDatabaseInitializer : IDatabaseInitializer<EmployeeContext>
    {
        //public void Initialize(DatabaseFacade database, bool force)
        public void Initialize(DbContext context, bool force)
        {
            if (context.AllMigrationsApplied())
            {
                if (!context.Set<Employee>().Any())
                {
                    //TODO Perform seeds
                }
            }
        }
    }
}