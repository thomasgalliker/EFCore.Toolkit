using Autofac;
using ToolkitSample.DataAccess.Contracts.Repository;
using ToolkitSample.DataAccess.Modularity;

namespace ToolkitSample.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<DataAccessModule>();
            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                var employeeRepository = scope.Resolve<IEmployeeRepository>();
                var employees = employeeRepository.GetAll();
                foreach (var employee in employees)
                {
                    System.Console.WriteLine($"Id={employee.Id}, FirstName={employee.FirstName}, LastName={employee.LastName}");
                }
            }
        }
    }
}
