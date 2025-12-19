using Autofac;
using EFCore.Toolkit;
using EFCore.Toolkit.Abstractions;
using ToolkitSample.DataAccess.Context;
using ToolkitSample.DataAccess.Contracts.Repository;
using ToolkitSample.DataAccess.Repository;
using ToolkitSample.DataAccess.Seed;
using ToolkitSample.Model;

namespace ToolkitSample.DataAccess.Modularity
{
    public class DataAccessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register all data seeds:
            builder.RegisterType<ApplicationSettingDataSeed>().As<IDataSeed>().SingleInstance();
            builder.RegisterType<DepartmentDataSeed>().As<IDataSeed>().SingleInstance();
            builder.RegisterType<CountryDataSeed>().As<IDataSeed>().SingleInstance();

            // Register an IDbConnection and an IDatabaseInitializer which are used to be injected into EmployeeContext
            builder.RegisterType<EmployeeContextDatabaseInitializer>().As<IDatabaseInitializer>().SingleInstance();

            // Finally, register the context all the repositories as InstancePerDependency
            builder.RegisterType<EmployeeContext>()
                .As<IDbContext>()
                .As<IEmployeeContext>()
                .AsSelf()
                .WithParameter("dbContextOptions", EmployeeContextDbContextOptions.Create<EmployeeContext>())
                .InstancePerDependency();

            builder.RegisterType<EmployeeRepository>().As<IEmployeeRepository>().InstancePerDependency();
            builder.RegisterType<GenericRepository<Country>>().As<IGenericRepository<Country>>().InstancePerDependency();
            builder.RegisterType<GenericRepository<Department>>().As<IGenericRepository<Department>>().InstancePerDependency();
        }
    }
}