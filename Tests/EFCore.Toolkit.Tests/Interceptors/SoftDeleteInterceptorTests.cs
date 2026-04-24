using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Testing;
using EFCore.Toolkit.Tests.Extensions;
using FluentAssertions;
using ToolkitSample.DataAccess.Context;
using ToolkitSample.Model;

using Xunit;
using Xunit.Abstractions;
using static EFCore.Toolkit.Tests.Stubs.Testdata;

namespace EFCore.Toolkit.Tests.Repositories
{
    /// <summary>
    /// Repository tests using <see cref="EmployeeContextTestDbConnection" /> as database connection.
    /// </summary>
    [Trait(Traits.Category, Traits.IntegrationTests)]
    [Collection("DbContextTests")]
    public class SoftDeleteInterceptorTests : ContextTestBase<EmployeeContext, EmployeeContextTestDbConnection<EmployeeContext>>
    {
        public SoftDeleteInterceptorTests(ITestOutputHelper testOutputHelper)
            : base(databaseInitializer: new CreateDatabaseIfNotExists(),
                  log: testOutputHelper.WriteLine)
        {
        }

        [Fact]
        public async Task ShouldSoftDelete()
        {
            // Arrange
            var countries = Countries.GetAll().ToArray();

            using (IGenericRepository<Country> countryRepository = new GenericRepository<Country>(this.CreateContext()))
            {
                countryRepository.AddRange(countries);
                countryRepository.Save();
            }

            // Act
            ChangeSet committedChangeSet;
            using (IGenericRepository<Country> countryRepository = new GenericRepository<Country>(this.CreateContext()))
            {
                foreach (var countryToDelete in countries)
                {
                    countryRepository.Remove(countryToDelete);
                }

                committedChangeSet = await countryRepository.SaveAsync();
            }

            // Assert
            committedChangeSet.Assert(expectedNumberOfAdded: 0, expectedNumberOfModified: 0, expectedNumberOfDeleted: countries.Length);

            using (IGenericRepository<Country> countryRepository = new GenericRepository<Country>(this.CreateContext()))
            {
                var allCountries = countryRepository.GetAll();
                allCountries.Should().HaveCount(countries.Length);

                foreach (var country in allCountries)
                {
                    country.IsDeleted.Should().BeTrue();
                }
            }
        }
    }
}