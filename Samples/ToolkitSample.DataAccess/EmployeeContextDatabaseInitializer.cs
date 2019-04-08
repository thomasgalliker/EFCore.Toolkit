using System.Collections.Generic;
using System.Linq;
using EFCore.Toolkit;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Extensions;
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
            context.Database.EnsureCreated();
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