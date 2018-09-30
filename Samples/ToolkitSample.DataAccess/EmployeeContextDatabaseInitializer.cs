using System.Collections.Generic;
using System.Linq;
using EntityFramework.Toolkit.EFCore;
using EntityFramework.Toolkit.EFCore.Contracts;
using EntityFramework.Toolkit.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;
using ToolkitSample.DataAccess.Context;
using ToolkitSample.Model;

namespace ToolkitSample.DataAccess
{
    public class EmployeeContextDatabaseInitializer : IDatabaseInitializer<EmployeeContext>
    {
        private readonly IEnumerable<IDataSeed> dataSeeds;

        public EmployeeContextDatabaseInitializer(IEnumerable<IDataSeed> dataSeeds)
        {
            this.dataSeeds = dataSeeds;
        }

        public void Initialize(DbContext context, bool force)
        {
            if (context.AllMigrationsApplied())
            {
                if (!context.Set<Employee>().Any())
                {
                    context.Seed(this.dataSeeds);
                }
            }
        }
    }
}