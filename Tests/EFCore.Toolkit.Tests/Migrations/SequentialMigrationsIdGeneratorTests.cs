using AwesomeAssertions;
using EFCore.Toolkit.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace EFCore.Toolkit.Tests.Migrations
{
    [Trait(Traits.Category, Traits.UnitTests)]
    public class SequentialMigrationsIdGeneratorTests
    {
        [Fact]
        public void GenerateId_NoExistingVersionMigration_GeneratesFirstVersion()
        {
            // Arrange
            var generator = new SequentialMigrationsIdGenerator<EmptyMigrationTestContext>();

            // Act
            string id = generator.GenerateId("InitialCreate");

            // Assert
            id.Should().Be("00000000000001_InitialCreate");
        }

        [Fact]
        public void GenerateId_ExistingVersionMigrations_GeneratesNextVersion()
        {
            // Arrange
            var generator = new SequentialMigrationsIdGenerator<VersionedMigrationTestContext>();

            // Act
            string id = generator.GenerateId("AddTrips");

            // Assert
            id.Should().Be("000004_AddTrips");
        }

        [Fact]
        public void GenerateId_LegacyTimestampMigrationsWithAutomaticTransition_GeneratesNextSequenceStart()
        {
            // Arrange
            var generator = new SequentialMigrationsIdGenerator<LegacyTimestampMigrationTestContext>();

            // Act
            string id = generator.GenerateId("AddNextThing");

            // Assert
            id.Should().Be("30000000000000_AddNextThing");
        }

        [Fact]
        public void GenerateId_ExistingTransitionVersion_GeneratesNextTransitionVersion()
        {
            // Arrange
            var generator = new SequentialMigrationsIdGenerator<TransitionedMigrationTestContext>();

            // Act
            string id = generator.GenerateId("AddAnotherThing");

            // Assert
            id.Should().Be("30000000000001_AddAnotherThing");
        }

        [Theory]
        [InlineData("1_InitialCreate", "InitialCreate")]
        [InlineData("000001_InitialCreate", "InitialCreate")]
        [InlineData("42_Add_Trips", "Add_Trips")]
        [InlineData("20260122115839_InitialCreate", "InitialCreate")]
        public void GetName_ValidId_ReturnsName(string id, string expectedName)
        {
            // Arrange
            var generator = new SequentialMigrationsIdGenerator<EmptyMigrationTestContext>();

            // Act
            string name = generator.GetName(id);

            // Assert
            name.Should().Be(expectedName);
        }

        [Theory]
        [InlineData("1_InitialCreate", true)]
        [InlineData("000001_InitialCreate", true)]
        [InlineData("123_AddTrips", true)]
        [InlineData("20260122115839_InitialCreate", true)]
        [InlineData("0_InitialCreate", false)]
        [InlineData("InitialCreate", false)]
        [InlineData("test2", false)]
        public void IsValidId_Value_ReturnsExpectedResult(string id, bool expectedResult)
        {
            // Arrange
            var generator = new SequentialMigrationsIdGenerator<EmptyMigrationTestContext>();

            // Act
            bool isValid = generator.IsValidId(id);

            // Assert
            isValid.Should().Be(expectedResult);
        }

        private sealed class EmptyMigrationTestContext : DbContext
        {
        }

        private sealed class VersionedMigrationTestContext : DbContext
        {
        }

        private sealed class LegacyTimestampMigrationTestContext : DbContext
        {
        }

        private sealed class TransitionedMigrationTestContext : DbContext
        {
        }

        [DbContext(typeof(VersionedMigrationTestContext))]
        [Migration("000001_InitialCreate")]
        private sealed class InitialCreateMigration : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
            }

            protected override void Down(MigrationBuilder migrationBuilder)
            {
            }
        }

        [DbContext(typeof(VersionedMigrationTestContext))]
        [Migration("000003_AddUsers")]
        private sealed class AddUsersMigration : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
            }

            protected override void Down(MigrationBuilder migrationBuilder)
            {
            }
        }

        [DbContext(typeof(LegacyTimestampMigrationTestContext))]
        [Migration("20260122115839_LegacyInitialCreate")]
        private sealed class LegacyTimestampInitialCreateMigration : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
            }

            protected override void Down(MigrationBuilder migrationBuilder)
            {
            }
        }

        [DbContext(typeof(TransitionedMigrationTestContext))]
        [Migration("20260122115839_LegacyInitialCreate")]
        private sealed class TransitionedLegacyInitialCreateMigration : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
            }

            protected override void Down(MigrationBuilder migrationBuilder)
            {
            }
        }

        [DbContext(typeof(TransitionedMigrationTestContext))]
        [Migration("30000000000000_AddNextThing")]
        private sealed class TransitionedAddNextThingMigration : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
            }

            protected override void Down(MigrationBuilder migrationBuilder)
            {
            }
        }
    }
}
