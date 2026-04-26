using System.Diagnostics;
using EFCore.Toolkit;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Extensions;
using Microsoft.EntityFrameworkCore;

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

            var connectionString = dbContext.Database.GetConnectionString();
            Debug.WriteLine($"Initializing database with connectionString={connectionString}");

            if (!dbContext.AllMigrationsApplied())
            {
                dbContext.Database.Migrate();
            }

            foreach (var dataSeed in this.dataSeeds)
            {
                dataSeed.Seed(dbContext);
            }

            dbContext.SaveChanges();
        }
    }
}