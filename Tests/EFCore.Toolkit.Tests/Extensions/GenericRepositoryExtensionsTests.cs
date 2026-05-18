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
    public class GenericRepositoryExtensionsTests : ContextTestBase<EmployeeContext, EmployeeContextTestDbConnection<EmployeeContext>>
    {
        public GenericRepositoryExtensionsTests(ITestOutputHelper testOutputHelper)
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
        public void RemoveWhere_HardDelete_RemovesMatchingRowsAndReturnsCount()
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
                affected = employeeRepository.RemoveWhere(e => e.FirstName == "Thomas");
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
        public async Task RemoveWhereAsync_DeletableEntity_HardDeletesAndReturnsCount()
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
                all.Should().HaveCount(2);
                all.Should().NotContain(c => c.Id == "ch");
                all.Should().OnlyContain(c => c.IsDeleted == false);
            }
        }

        [Fact]
        public void RemoveWhere_DeletableEntity_HardDeletesAndReturnsCount()
        {
            // Arrange
            var countries = new[]
            {
                new Country { Id = "rw1", Name = "RemoveWhere 1" },
                new Country { Id = "rw2", Name = "RemoveWhere 2" },
                new Country { Id = "rw3", Name = "RemoveWhere 3" }
            };

            using (var context = this.CreateContext())
            {
                context.Set<Country>().AddRange(countries);
                context.SaveChanges();
            }

            // Act
            int affected;
            using (IGenericRepository<Country> countryRepository = new GenericRepository<Country>(this.CreateContext()))
            {
                affected = countryRepository.RemoveWhere(c => c.Id == "rw1");
            }

            // Assert
            affected.Should().Be(1);

            using (var context = this.CreateContext())
            {
                var all = context.Set<Country>().IgnoreQueryFilters().ToList();
                all.Should().HaveCount(2);
                all.Should().NotContain(c => c.Id == "rw1");
                all.Should().OnlyContain(c => c.IsDeleted == false);
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
        public void RemoveWhere_NullPredicate_Throws()
        {
            using IGenericRepository<Employee> employeeRepository = new GenericRepository<Employee>(this.CreateContext());

            var act = () => employeeRepository.RemoveWhere(null!);

            act.Should().Throw<ArgumentNullException>();
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
            await context.Set<Employee>().Where(e => e.FirstName == "Thomas").ExecuteDeleteAsync();

            context.ChangeTracker.Entries().Should().BeEmpty();
        }
    }
}
