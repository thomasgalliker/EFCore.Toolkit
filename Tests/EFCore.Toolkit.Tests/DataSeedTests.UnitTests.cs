using FluentAssertions;

using ToolkitSample.DataAccess.Seed;

using Xunit;

namespace EFCore.Toolkit.Tests
{
    [Trait(Traits.Category, Traits.UnitTests)]
    public class DataSeedUnitTests
    {
        [Fact]
        public void ShouldGetAll()
        {
            // Arrange
            var departmentDataSeed = new DepartmentDataSeed();

            // Act
            var allDepartments = departmentDataSeed.GetAll();

            // Assert
            allDepartments.Should().NotBeNull();
            allDepartments.Should().HaveCount(2);
        }
    }
}