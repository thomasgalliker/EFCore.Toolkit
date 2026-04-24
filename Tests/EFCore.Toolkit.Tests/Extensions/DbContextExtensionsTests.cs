using EFCore.Toolkit.Extensions;
using EFCore.Toolkit.Testing;
using EFCore.Toolkit.Tests.Stubs;

using FluentAssertions;
using ToolkitSample.DataAccess.Context;
using ToolkitSample.Model;

using Xunit;
using Xunit.Abstractions;
using static EFCore.Toolkit.Tests.Stubs.Testdata;

namespace EFCore.Toolkit.Tests.Extensions
{
    [Trait(Traits.Category, Traits.IntegrationTests)]
    [Collection("DbContextTests")]
    public class DbContextExtensionsTests : ContextTestBase<EmployeeContext>
    {
        private readonly ITestOutputHelper testOutputHelper;

        public DbContextExtensionsTests(ITestOutputHelper testOutputHelper)
            : base(dbContextOptions: EmployeeContextTestDbConnection.CreateDbContextOptions<EmployeeContext>(),
                  databaseInitializer: new CreateDatabaseIfNotExists(),
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

        [Fact]
        public async Task ShouldAddOrUpdate_WithExpression()
        {
            // Arrange
            var departments1 = Testdata.Departments.GetAll().ToArray();
            var departments2 = Testdata.Departments.GetAll()
                .Append(new Department { Name = "Test" })
                .ToArray();

            foreach (var department in departments2)
            {
                department.Description = "Updated description";
            }

            // Act
            using (IEmployeeContext employeeContext = this.CreateContext())
            {
                employeeContext.AddOrUpdate(departments1, d => d.Name);
                await employeeContext.SaveChangesAsync();
            }
            
            using (IEmployeeContext employeeContext = this.CreateContext())
            {
                employeeContext.AddOrUpdate(departments2, d => d.Name);
                await employeeContext.SaveChangesAsync();
            }

            // Assert
            using (IEmployeeContext employeeContext = this.CreateContext())
            {
                var allDepartments = employeeContext.Set<Department>().ToArray();
                allDepartments.Should().HaveCount(departments2.Length);
                allDepartments.Select(d => d.Description).All(d => d == "Updated description").Should().BeTrue();
            }
        }

        [Fact]
        public async Task ShouldAddOrUpdate_WithPrimaryKey()
        {
            // Arrange
            var countries1 = Testdata.Countries.GetAll().ToArray();
            var countries2 = Testdata.Countries.GetAll().ToArray();

            // Act
            using (IEmployeeContext employeeContext = this.CreateContext())
            {
                employeeContext.AddOrUpdate(countries1);
                await employeeContext.SaveChangesAsync();
            }
            
            using (IEmployeeContext employeeContext = this.CreateContext())
            {
                employeeContext.AddOrUpdate(countries2);
                await employeeContext.SaveChangesAsync();
            }

            // Assert
            using (IEmployeeContext employeeContext = this.CreateContext())
            {
                countries1.Should().BeEquivalentTo(countries2);

                var allCountries = employeeContext.Set<Country>().ToArray();
                allCountries.Should().HaveCount(countries1.Length);
            }
        }
    }
}