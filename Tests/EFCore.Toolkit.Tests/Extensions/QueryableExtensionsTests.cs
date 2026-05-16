using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Extensions;
using EFCore.Toolkit.Testing;
using EFCore.Toolkit.Tests.Stubs;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using ToolkitSample.DataAccess.Context;
using ToolkitSample.DataAccess.Repository;
using ToolkitSample.Model;
using Xunit;
using Xunit.Abstractions;
using static EFCore.Toolkit.Tests.Stubs.Testdata.Employees;

namespace EFCore.Toolkit.Tests.Extensions
{
    [Trait(Traits.Category, Traits.IntegrationTests)]
    [Collection("DbContextTests")]
    public class QueryableExtensionsTests : ContextTestBase<EmployeeContext, EmployeeContextTestDbConnection<EmployeeContext>>
    {
        public QueryableExtensionsTests(ITestOutputHelper testOutputHelper)
            : base(databaseInitializer: new CreateDatabaseIfNotExists(),
                  log: testOutputHelper.WriteLine)
        {
        }

        [Fact]
        public async Task RemoveWhereAsync_HardDelete_RemovesMatchingRowsAndReturnsCount()
        {
            // Arrange
            var employees = new List<Employee> { CreateEmployee1(), CreateEmployee2(), CreateEmployee3() };

            using (IGenericRepository<Employee> employeeRepository = new GenericRepository<Employee>(this.CreateContext()))
            {
                employeeRepository.AddRange(employees);
                employeeRepository.Save();
            }

            // Act
            int affected;
            using (IGenericRepository<Employee> employeeRepository = new GenericRepository<Employee>(this.CreateContext()))
            {
                affected = await employeeRepository.RemoveWhereAsync(e => e.FirstName == "Thomas");
            }

            // Assert
            affected.Should().Be(1);

            using (IGenericRepository<Employee> employeeRepository = new GenericRepository<Employee>(this.CreateContext()))
            {
                var remaining = employeeRepository.GetAll().ToList();
                remaining.Should().HaveCount(2);
                remaining.Should().NotContain(e => e.FirstName == "Thomas");
            }
        }

        [Fact]
        public async Task RemoveWhereAsync_SoftDelete_SetsIsDeletedAndReturnsCount()
        {
            // Arrange
            var countries = Testdata.Countries.GetAll().ToArray();

            using (var context = this.CreateContext())
            {
                context.Set<Country>().AddRange(countries);
                await context.SaveChangesAsync();
            }

            // Act
            int affected;
            using (IGenericRepository<Country> countryRepository = new GenericRepository<Country>(this.CreateContext()))
            {
                affected = await countryRepository.RemoveWhereAsync(c => c.Id == "ch");
            }

            // Assert
            affected.Should().Be(1);

            using (var context = this.CreateContext())
            {
                var all = await context.Set<Country>().IgnoreQueryFilters().ToListAsync();
                all.Should().HaveCount(3);
                all.Single(c => c.Id == "ch").IsDeleted.Should().BeTrue();
                all.Where(c => c.Id != "ch").Should().OnlyContain(c => c.IsDeleted == false);
            }
        }

        [Fact]
        public async Task RemoveWhereAsync_NoMatches_ReturnsZero()
        {
            // Arrange
            using (IGenericRepository<Employee> employeeRepository = new GenericRepository<Employee>(this.CreateContext()))
            {
                employeeRepository.AddRange(new[] { CreateEmployee1(), CreateEmployee2() });
                employeeRepository.Save();
            }

            // Act
            int affected;
            using (IGenericRepository<Employee> employeeRepository = new GenericRepository<Employee>(this.CreateContext()))
            {
                affected = await employeeRepository.RemoveWhereAsync(e => e.FirstName == "Nobody");
            }

            // Assert
            affected.Should().Be(0);

            using (IGenericRepository<Employee> employeeRepository = new GenericRepository<Employee>(this.CreateContext()))
            {
                employeeRepository.GetAll().ToList().Should().HaveCount(2);
            }
        }

        [Fact]
        public async Task RemoveWhereAsync_NullPredicate_Throws()
        {
            using IGenericRepository<Employee> employeeRepository = new GenericRepository<Employee>(this.CreateContext());

            var act = () => employeeRepository.RemoveWhereAsync(null!);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task RemoveWhereAsync_DoesNotPopulateChangeTracker()
        {
            // Arrange
            using (IGenericRepository<Employee> employeeRepository = new GenericRepository<Employee>(this.CreateContext()))
            {
                employeeRepository.AddRange(new[] { CreateEmployee1(), CreateEmployee2() });
                employeeRepository.Save();
            }

            // Act / Assert
            using var context = this.CreateContext();
            await context.Set<Employee>().RemoveWhereAsync(e => e.FirstName == "Thomas");

            context.ChangeTracker.Entries().Should().BeEmpty();
        }
    }
}
