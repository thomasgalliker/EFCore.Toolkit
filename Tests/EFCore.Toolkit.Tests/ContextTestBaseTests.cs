using EFCore.Toolkit;
using EFCore.Toolkit.Contracts;
using EFCore.Toolkit.Testing;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Toolkit.Tests
{
    public class ContextTestBaseTests_DbConnectionStringOnly : ContextTestBase<ContextTestBaseTests_DbConnectionStringOnly.TestContext>
    {
        public ContextTestBaseTests_DbConnectionStringOnly()
            : base(dbConnectionString: () => @"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\EF.Toolkit.ContextTestBaseTests.mdf; Integrated Security=True;".RandomizeDatabaseName())
        {
        }

        [Fact]
        public void ShouldCreateContextWithCtorParameters()
        {
            // Arrange

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

    public class ContextTestBaseTests_DbConnectionOnly : ContextTestBase<ContextTestBaseTests_DbConnectionOnly.TestContext>
    {
        public ContextTestBaseTests_DbConnectionOnly()
            : base(dbConnection: () => new DbConnection(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\EF.Toolkit.ContextTestBaseTests.mdf; Integrated Security=True;".RandomizeDatabaseName()))
        {
        }

        [Fact]
        public void ShouldCreateContextWithCtorParameters()
        {
            // Arrange

            // Act
            var testContext = this.CreateContext();

            // Assert
            testContext.Should().BeOfType<TestContext>();
        }

        public class TestContext : DbContextBase<TestContext>
        {
            public TestContext(IDbConnection dbConnection, IDatabaseInitializer<TestContext> databaseInitializer)
                : base(dbConnection, databaseInitializer)
            {
            }
        }
    }
}