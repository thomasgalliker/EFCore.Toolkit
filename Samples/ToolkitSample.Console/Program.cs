using System;
using System.Linq;
using Autofac;
using ToolkitSample.DataAccess.Contracts.Repository;
using ToolkitSample.DataAccess.Modularity;
using ToolkitSample.Model;

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

                if (!employees.Any())
                {
                    employeeRepository.Add(new Employee { FirstName = "Thomas", LastName = "Galliker", Birthdate = new DateTime(1986, 07, 11), EmployementDate = new DateTime(2000, 1, 1) });
                    employeeRepository.Save();

                    employees = employeeRepository.GetAll();
                }

                foreach (var employee in employees)
                {
                    System.Console.WriteLine($"Id={employee.Id}, FirstName={employee.FirstName}, LastName={employee.LastName}");
                }
            }

            System.Console.ReadKey();
        }
    }
}