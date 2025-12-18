using EFCore.Toolkit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Toolkit.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static void SeedData<TDbContext>(this IServiceProvider services) where TDbContext : DbContextBase
        {
            using (var serviceScope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = services.GetRequiredService<TDbContext>();
                var dataSeeds = services.GetService<IEnumerable<IDataSeed>>();
                if (dataSeeds != null)
                {
                    foreach (var dataSeed in dataSeeds)
                    {
                        dataSeed.Seed(dbContext);
                    }
                }
            }
        }
    }
}