﻿using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Extensions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ToolkitSample.DataAccess.Context.Auditing;

using Xunit;

namespace EFCore.Toolkit.Tests.Extensions
{
    public class TypeExtensionsTests
    {
        [Fact]
        public void ShouldGetFormattedName()
        {
            // Arrange
            var type = typeof(Func<bool, string, int>);

            // Act
            var formattedName = type.GetFormattedName();

            // Assert
            formattedName.Should().Be("Func<Boolean, String, Int32>");
        }

        [Fact]
        public void ShouldGetFormattedFullName()
        {
            // Arrange
            var type = typeof(IList<string>);

            // Act
            var formattedName = type.GetFormattedFullname();

            // Assert
            formattedName.Should().Be("System.Collections.Generic.IList<System.String>");
        }

        [Fact]
        public void ShouldThrowExceptionIfCouldNotFindMatchingConstructorWithNoParameters()
        {
            // Arrange
            var args = new object[] { };

            // Act
            Action action = () => typeof(TestAuditDbContext).GetMatchingConstructor(args);

            // Assert
            action.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain("TestAuditDbContext does not have a constructor with no parameters.");
        }

        [Fact]
        public void ShouldThrowExceptionIfCouldNotFindMatchingConstructorWithParameters()
        {
            // Arrange
            var args = new object[] { 111f, 222m };

            // Act
            Action action = () => typeof(TestAuditDbContext).GetMatchingConstructor(args);

            // Assert
            action.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain("TestAuditDbContext does not have a constructor with parameters (Single, Decimal).");
        }

        [Fact]
        public void ShouldGetConstructorWithOptionalParameter()
        {
            // Arrange
            var args = new object[]
            {
                EmployeeContextTestDbConnection.CreateDbContextOptions<TestAuditDbContext>(),
                new DropCreateDatabaseAlways<TestAuditDbContext>(),
            };

            // Act
            var contextCtor = typeof(TestAuditDbContext).GetMatchingConstructor(args);

            // Assert
            var contextCtorParameters = contextCtor.ConstructorInfo.GetParameters();
            contextCtorParameters.Should().HaveCount(2);
            contextCtorParameters.ElementAt(0).ParameterType.Should().Be(typeof(DbContextOptions));
            contextCtorParameters.ElementAt(1).ParameterType.Should().Be(typeof(IDatabaseInitializer<TestAuditDbContext>));

            var testContext = contextCtor.Invoke();
            testContext.Should().BeOfType<TestAuditDbContext>();
        }
    }
}