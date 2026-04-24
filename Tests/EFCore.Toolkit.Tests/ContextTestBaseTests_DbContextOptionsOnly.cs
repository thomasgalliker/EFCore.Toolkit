using EFCore.Toolkit.Testing;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Toolkit.Tests
{
    [Trait(Traits.Category, Traits.IntegrationTests)]
    [Collection("DbContextTests")]
    public class ContextTestBaseTests_DbContextOptionsOnly : ContextTestBase<ContextTestBaseTests_DbContextOptionsOnly.TestContext>
    {
        public ContextTestBaseTests_DbContextOptionsOnly()
            : base(dbContextOptions: EmployeeContextTestDbConnection.CreateDbContextOptions<TestContext>())
        {
        }

        [Fact]
        public void ShouldCreateContextWithCtorParameters()
        {
            // Act
            var testContext = this.CreateContext();

            // Assert
            testContext.Should().BeOfType<TestContext>();
        }

        public class TestContext : DbContextBase
        {
            public TestContext(DbContextOptions dbContextOptions, IDatabaseInitializer databaseInitializer)
                : base(dbContextOptions, databaseInitializer)
            {
            }
        }
    }
}