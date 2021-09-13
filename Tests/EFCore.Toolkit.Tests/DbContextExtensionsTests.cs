using System.Collections.Generic;
using System.Threading.Tasks;
using EFCore.Toolkit.Extensions;
using EFCore.Toolkit.Testing;
using EFCore.Toolkit.Tests.Stubs;

using FluentAssertions;
using ToolkitSample.DataAccess.Context;
using ToolkitSample.Model;

using Xunit;
using Xunit.Abstractions;

namespace EFCore.Toolkit.Tests
{
    public class DbContextExtensionsTests : ContextTestBase<EmployeeContext>
    {
        private readonly ITestOutputHelper testOutputHelper;

        public DbContextExtensionsTests(ITestOutputHelper testOutputHelper)
            : base(dbContextOptions: EmployeeContextTestDbConnection.CreateDbContextOptions<EmployeeContext>(),
                  databaseInitializer: new CreateDatabaseIfNotExists<EmployeeContext>(),
                   log: testOutputHelper.WriteLine)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task ShouldGetGetTableRowCounts()
        {
            // Arrange
            List<TableRowCounts> tableRowCounts;

            using (var employeeContext = this.CreateContext())
            {
                employeeContext.Set<Employee>().Add(Testdata.Employees.CreateEmployee1());
                employeeContext.Set<Employee>().Add(Testdata.Employees.CreateEmployee2());
                employeeContext.Set<Employee>().Add(Testdata.Employees.CreateEmployee3());
                employeeContext.SaveChanges();

                // Act
                tableRowCounts = await employeeContext.GetTableRowCountsAsync();
            }

            // Assert
            this.testOutputHelper.WriteLine(ObjectDumper.Dump(tableRowCounts, DumpStyle.CSharp));

            tableRowCounts.Should().HaveCount(6);
            tableRowCounts.Should().ContainSingle(r => r.TableName == "[dbo].[ApplicationSetting]" && r.TableRowCount == 0);
            tableRowCounts.Should().ContainSingle(r => r.TableName == "[dbo].[Country]" && r.TableRowCount == 0);
            tableRowCounts.Should().ContainSingle(r => r.TableName == "[dbo].[Department]" && r.TableRowCount == 0);
            tableRowCounts.Should().ContainSingle(r => r.TableName == "[dbo].[EmployeeAudit]" && r.TableRowCount == 0);
            tableRowCounts.Should().ContainSingle(r => r.TableName == "[dbo].[Person]" && r.TableRowCount == 3);
            tableRowCounts.Should().ContainSingle(r => r.TableName == "[dbo].[Room]" && r.TableRowCount == 0);
        }
    }
}
