using EFCore.Toolkit;
using EFCore.Toolkit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ToolkitSample.DataAccess.Context;
using ToolkitSample.DataAccess.Contracts.Repository;
using ToolkitSample.DataAccess.Repository;
using ToolkitSample.DataAccess.Seed;
using ToolkitSample.Model;

namespace ToolkitSample.DataAccess.Modularity
{
    public static class DataAccessServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services)
        {
            services.AddSingleton<IDataSeed, ApplicationSettingDataSeed>();
            services.AddSingleton<IDataSeed, DepartmentDataSeed>();
            services.AddSingleton<IDataSeed, CountryDataSeed>();

            services.AddSingleton<IDatabaseInitializer, EmployeeContextDatabaseInitializer>();

            services.AddTransient<EmployeeContext>(sp =>
                new EmployeeContext(
                    EmployeeContextDbContextOptions.Create<EmployeeContext>(),
                    sp.GetRequiredService<IDatabaseInitializer>()));
            services.AddTransient<IDbContext>(sp => sp.GetRequiredService<EmployeeContext>());
            services.AddTransient<IEmployeeContext>(sp => sp.GetRequiredService<EmployeeContext>());

            services.AddTransient<IEmployeeRepository, EmployeeRepository>();
            services.AddTransient<IGenericRepository<Country>, GenericRepository<Country>>();
            services.AddTransient<IGenericRepository<Department>, GenericRepository<Department>>();

            return services;
        }
    }
}
