using EFCore.Toolkit.Testing;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Toolkit.Tests
{

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

        public class TestContext : DbContextBase<TestContext>
        {
            public TestContext(DbContextOptions dbContextOptions, IDatabaseInitializer<TestContext> databaseInitializer)
                : base(dbContextOptions, databaseInitializer)
            {
            }
        }
    }
}