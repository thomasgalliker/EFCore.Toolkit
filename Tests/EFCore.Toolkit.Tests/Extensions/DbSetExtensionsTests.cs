using System.Linq;
using System.Threading.Tasks;
using EFCore.Toolkit.Extensions;
using EFCore.Toolkit.Testing;
using EFCore.Toolkit.Tests.Stubs;

using FluentAssertions;
using ToolkitSample.DataAccess.Context;
using ToolkitSample.Model;

using Xunit;
using Xunit.Abstractions;

namespace EFCore.Toolkit.Tests.Extensions
{
    [Collection("DbContextTests")]
    public class DbSetExtensionsTests : ContextTestBase<EmployeeContext>
    {
        private readonly ITestOutputHelper testOutputHelper;

        public DbSetExtensionsTests(ITestOutputHelper testOutputHelper)
            : base(dbContextOptions: EmployeeContextTestDbConnection.CreateDbContextOptions<EmployeeContext>(),
                  databaseInitializer: new CreateDatabaseIfNotExists(),
                   log: testOutputHelper.WriteLine)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task ShouldAddOrUpdate_UsingPrimaryKeyComparison()
        {
            // Arrange
            var countries = Testdata.Countries.GetAll().ToArray();

            // Act
            using (IEmployeeContext employeeContext = this.CreateContext())
            {
                var dbSet = employeeContext.Set<Country>();
                dbSet.AddOrUpdate(countries);
                await employeeContext.SaveChangesAsync();

                dbSet.AddOrUpdate(countries);
                await employeeContext.SaveChangesAsync();
            }

            // Assert
            using (IEmployeeContext employeeContext = this.CreateContext())
            {
                var allCountries = employeeContext.Set<Country>().ToArray();
                allCountries.Should().HaveCount(countries.Length);
            }
        }
        

        [Fact]
        public async Task ShouldAddOrUpdate_FilterDeletedItems()
        {
            // Arrange
            var country = Testdata.Countries.GetAll().ToArray()[0];

            // Act
            using (IEmployeeContext employeeContext = this.CreateContext())
            {
                var dbSet = employeeContext.Set<Country>();
                country = dbSet.AddOrUpdate(country);
                country.IsDeleted = true;
                await employeeContext.SaveChangesAsync();

                dbSet.AddOrUpdate(country);
                await employeeContext.SaveChangesAsync();
            }

            // Assert
            using (IEmployeeContext employeeContext = this.CreateContext())
            {
                var allCountries = employeeContext.Set<Country>().ToArray();
                allCountries.Should().HaveCount(1);
            }
        }

        [Fact]
        public async Task ShouldAddOrUpdate_UsingCustomKeyComparison()
        {
            // Arrange
            var countries = Testdata.Countries.GetAll().ToArray();

            // Act
            using (IEmployeeContext employeeContext = this.CreateContext())
            {
                var dbSet = employeeContext.Set<Country>();
                dbSet.AddOrUpdate(countries, c => c.Name);
                await employeeContext.SaveChangesAsync();

                dbSet.AddOrUpdate(countries, c => c.Name);
                await employeeContext.SaveChangesAsync();
            }

            // Assert
            using (IEmployeeContext employeeContext = this.CreateContext())
            {
                var allCountries = employeeContext.Set<Country>().ToArray();
                allCountries.Should().HaveCount(countries.Length);
            }
        }
    }
}
