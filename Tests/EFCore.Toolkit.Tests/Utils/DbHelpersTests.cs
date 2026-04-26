using System.Linq.Expressions;
using EFCore.Toolkit.Utils;
using AwesomeAssertions;
using ToolkitSample.Model;
using Xunit;

namespace EFCore.Toolkit.Tests.Utils
{
    [Trait(Traits.Category, Traits.UnitTests)]
    public class DbHelpersTests
    {
        [Fact]
        public void ShouldParseStringMemberExpression()
        {
            // Arrange
            Expression<Func<Company, string>> expr = c => c.Name!;

            // Act
            var isParsed = DbHelpers.TryParsePath(expr.Body, out var path);

            // Assert
            isParsed.Should().BeTrue();
            path.Should().Be("Name");
        }

        [Fact]
        public void ShouldParseObjectMemberExpression()
        {
            // Arrange
            Expression<Func<Company, object>> expr = c => c.BaseProperty!;

            // Act
            var isParsed = DbHelpers.TryParsePath(expr.Body, out var path);

            // Assert
            isParsed.Should().BeTrue();
            path.Should().Be("BaseProperty");
        }

        [Fact]
        public void ShouldParseSelectCollectionMemberExpression()
        {
            // Arrange
            Expression<Func<Company, object>> expr = c => c.Employees!.Select(e => e.Department);

            // Act
            var isParsed = DbHelpers.TryParsePath(expr.Body, out var path);

            // Assert
            isParsed.Should().BeTrue();
            path.Should().Be("Employees.Department");
        }

        [Fact]
        public void ShouldParseDeeplyNestedSelectCollections()
        {
            // Arrange
            Expression<Func<Company, object>> expr = c => c.Employees!.Select(e => e.Department!.Employees.Select(d => d.FirstName));

            // Act
            var isParsed = DbHelpers.TryParsePath(expr.Body, out var path);

            // Assert
            isParsed.Should().BeTrue();
            path.Should().Be("Employees.Department.Employees.FirstName");
        }

        [Fact]
        public void ShouldParseRootParameterExpression()
        {
            // Arrange
            Expression<Func<Company, object>> expr = c => c;

            // Act
            var isParsed = DbHelpers.TryParsePath(expr.Body, out var path);

            // Assert
            isParsed.Should().BeTrue();
            path.Should().BeNull();
        }

        [Fact]
        public void ShouldParseConstantExpression()
        {
            // Arrange
            Expression<Func<Company, object>> expr = c => "constant";

            // Act
            var isParsed = DbHelpers.TryParsePath(expr.Body, out var path);

            // Assert
            isParsed.Should().BeTrue();
            path.Should().BeNull();
        }

        [Fact]
        public void ShouldParseConvertExpression()
        {
            // Arrange
            Expression<Func<Company, object>> expr = c => (object)c.Name!;

            // Act
            var isParsed = DbHelpers.TryParsePath(expr.Body, out var path);

            // Assert
            isParsed.Should().BeTrue();
            path.Should().Be("Name");
        }


        [Fact]
        public void ShouldFailUnsupportedMethodCall()
        {
            // Arrange
            Expression<Func<Company, object>> expr = c => c.Employees!.FirstOrDefault()!;

            // Act
            var isParsed = DbHelpers.TryParsePath(expr.Body, out var path);

            // Assert
            isParsed.Should().BeFalse();
            path.Should().BeNull();
        }

        [Fact]
        public void ShouldParseChainedMemberSelectAndCast()
        {
            // Arrange
            Expression<Func<Company, object>> expr = c => (object)c.Employees!.Select(e => e.Department!.Name);

            // Act
            var isParsed = DbHelpers.TryParsePath(expr.Body, out var path);

            // Assert
            isParsed.Should().BeTrue();
            path.Should().Be("Employees.Department.Name");
        }
    }

    public class DerivedClass : BaseClass
    {
        public object? ObjectProperty { get; set; }
    }

    public class BaseClass
    {
    }

    public class Company
    {
        public string? Name { get; set; }

        public BaseClass? BaseProperty { get; set; }

        public ICollection<Employee>? Employees { get; set; }
    }
}