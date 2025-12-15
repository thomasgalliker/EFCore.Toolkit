using System.Collections.Generic;
using System.Linq;
using EFCore.Toolkit;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Extensions;
using Microsoft.EntityFrameworkCore;
using ToolkitSample.Model;

namespace ToolkitSample.DataAccess
{
    public class EmployeeContextDatabaseInitializer : IDatabaseInitializer
    {
        private readonly IEnumerable<IDataSeed> dataSeeds;

        public EmployeeContextDatabaseInitializer(IEnumerable<IDataSeed> dataSeeds)
        {
            this.dataSeeds = dataSeeds;
        }

        public void Initialize(DbContextBase dbContext, bool force)
        {

            dbContext.Database.EnsureCreated();

            if (dbContext.AllMigrationsApplied())
            {
                if (!dbContext.Set<Employee>().Any())
                {
                    foreach (var dataSeed in this.dataSeeds)
                    {
                        dataSeed.Seed(dbContext);
                    }

                    dbContext.SaveChanges();
                }
            }
        }
    }
}