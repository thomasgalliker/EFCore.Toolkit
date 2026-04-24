using EFCore.Toolkit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ToolkitSample.DataAccess.Contracts.Repository;
using ToolkitSample.DataAccess.Modularity;
using ToolkitSample.Model;

namespace ToolkitSample.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDataAccess();

            using var serviceProvider = serviceCollection.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var employeeRepository = scope.ServiceProvider.GetRequiredService<IEmployeeRepository>();
            var employees = employeeRepository.GetAll();

            if (!employees.Any())
            {
                var employee = new Employee
                {
                    FirstName = "Thomas",
                    LastName = "Galliker",
                    Birthdate = new DateTime(1986, 07, 11),
                    EmployementDate = new DateTime(2000, 1, 1)
                };
                employeeRepository.Add(employee);
                employeeRepository.Save();

                employees = employeeRepository.GetAll();
            }

            foreach (var employee in employees)
            {
                Console.WriteLine($"Id={employee.Id}, FirstName={employee.FirstName}, LastName={employee.LastName}");
            }

            Console.ReadKey();
        }
    }
}
