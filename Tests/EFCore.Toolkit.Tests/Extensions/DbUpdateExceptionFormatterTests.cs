using System;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Testing;
using EFCore.Toolkit.Tests.Auditing;
using EFCore.Toolkit.Tests.Stubs;
using EFCore.Toolkit.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ToolkitSample.DataAccess.Context;
using ToolkitSample.Model;

using Xunit;
using Xunit.Abstractions;

namespace EFCore.Toolkit.Tests.Extensions
{
    /// <summary>
    ///     Repository tests using <see cref="EmployeeContextTestDbConnection" /> as database connection.
    /// </summary>
    public class DbUpdateExceptionFormatterTests : ContextTestBase<EmployeeContext>
    {
        private readonly ITestOutputHelper testOutputHelper;

        public DbUpdateExceptionFormatterTests(ITestOutputHelper testOutputHelper)
            : base(dbConnection: () => new EmployeeContextTestDbConnection(),
                  databaseInitializer: new CreateDatabaseIfNotExists<EmployeeContext>(),
                  log: testOutputHelper.WriteLine)
        {
            this.testOutputHelper = testOutputHelper;

            AssemblyLoader.Current = new TestAssemblyLoader();
        }

        [Fact]
        public void ShouldThrowDbUpdateExceptionWithFormattedExceptionMessage()
        {
            // Arrange
            var employee = Testdata.Employees.CreateEmployee1();
            employee.DepartmentId = 99;
            employee.CountryId = "XX";

            // Act
            using (IGenericRepository<Employee> employeeRepository = new GenericRepository<Employee>(this.CreateContext()))
            {
                employeeRepository.Add(employee);
                Action action = () => employeeRepository.Save();

                // Assert
                action.Should().Throw<DbUpdateException>()
                    .Which.Message.Should()
                    .Contain("Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while updating the entries. See the inner exception for details. " +
                             "---> System.Data.SqlClient.SqlException: The INSERT statement conflicted with the FOREIGN KEY constraint \"FK_Person_Country_CountryId\". " +
                             "The conflict occurred in database \"EF.Toolkit.Tests_")
                    .And.Contain("table \"dbo.Country\", column 'Id'.");
            }
        }
    }
}