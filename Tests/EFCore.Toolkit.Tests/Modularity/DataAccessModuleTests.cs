using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using ToolkitSample.DataAccess.Contracts.Repository;
using ToolkitSample.DataAccess.Modularity;
using ToolkitSample.DataAccess.Repository;

using Xunit;

namespace EFCore.Toolkit.Tests.Modularity
{
    [Trait(Traits.Category, Traits.UnitTests)]
    public class DataAccessModuleTests
    {
        [Fact]
        public void ShouldBuildAndResolveDependencies()
        {
            // Arrange
            var provider = GetServiceProvider();

            // Act
            var employeeRepository = provider.GetRequiredService<IEmployeeRepository>();

            // Assert
            employeeRepository.Should().NotBeNull();
            employeeRepository.Should().BeOfType<EmployeeRepository>();
        }

        private static IServiceProvider GetServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddDataAccess();
            return services.BuildServiceProvider();
        }
    }
}
