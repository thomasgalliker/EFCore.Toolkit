using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit
{
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly IEnumerable<IDataSeed> dataSeeds;

        public DatabaseInitializer(IEnumerable<IDataSeed> dataSeeds)
        {
            this.dataSeeds = dataSeeds;
        }

        public void Initialize(DbContextBase context, bool force)
        {
            context.Database.EnsureCreated();

            if (!context.AllMigrationsApplied())
            {
                context.Database.Migrate();
            }

            foreach (var dataSeed in this.dataSeeds)
            {
                dataSeed.Seed(context);
            }

            context.SaveChanges();
        }
    }
}