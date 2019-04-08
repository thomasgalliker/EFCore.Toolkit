using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Extensions;
using EFCore.Toolkit.Testing;
using EFCore.Toolkit.Tests.Auditing;
using EFCore.Toolkit.Utils;
using FluentAssertions;
using ToolkitSample.DataAccess.Context;
using Xunit;
using Xunit.Abstractions;

namespace EFCore.Toolkit.Tests.Extensions
{
    public class DbConnectionExtensionsTests : ContextTestBase<EmployeeContext>
    {
        private static readonly IDbConnection dbConnection = new EmployeeContextTestDbConnection();

        public DbConnectionExtensionsTests(ITestOutputHelper testOutputHelper)
            : base(() => dbConnection, null, testOutputHelper.WriteLine)
        {
            AssemblyLoader.Current = new TestAssemblyLoader();
        }

        [Fact]
        public void ShouldDropDatabaseUsingDbConnection()
        {
            // Arrange
            using (var context = this.CreateContext())
            {
                context.Database.EnsureCreated();
            }

            // Act
            dbConnection.DropDatabase();

            // Assert
        }

        [Fact]
        public void ShouldGetDatabaseName()
        {
            // Arrange
            using (var context = this.CreateContext())
            {
                context.Database.EnsureCreated();
            }

            // Act
            var databaseName = dbConnection.GetDatabaseName();

            // Assert
            databaseName.Should().StartWith("EF.Toolkit.Tests_");
        }
    }
}