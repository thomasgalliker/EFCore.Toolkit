using EFCore.Toolkit.Extensions;
using AwesomeAssertions;

using Xunit;

namespace EFCore.Toolkit.Abstractions.Tests.Extensions
{
    [Trait(Traits.Category, Traits.UnitTests)]
    public class DateTimeExtensionsTests
    {
        [Fact]
        public void ShouldSpecifyKindLocalToUtc()
        {
            // Arrange
            var dateTimeNow = DateTime.Now;

            // Act
            var utcNow = dateTimeNow.SpecifyKind(DateTimeKind.Utc);

            // Assert
            utcNow.Kind.Should().Be(DateTimeKind.Utc);
            utcNow.Ticks.Should().Be(dateTimeNow.Ticks);
        }

        [Fact]
        public void ShouldConvertLocalToUtc()
        {
            // Arrange
            var dateTimeNow = DateTime.Now;

            // Act
            var utcNow = dateTimeNow.ToKindUtc();

            // Assert
            utcNow.Kind.Should().Be(DateTimeKind.Utc);
            utcNow.Ticks.Should().Be(dateTimeNow.ToUniversalTime().Ticks);
        }
    }
}