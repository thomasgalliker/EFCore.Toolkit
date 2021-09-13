using System;
using System.Linq;
using System.Reflection;
using EFCore.Toolkit.Auditing;
using EFCore.Toolkit.Utils;
using FluentAssertions;
using ToolkitSample.Model;
using ToolkitSample.Model.Auditing;
using Xunit;

namespace EFCore.Toolkit.Tests.Auditing
{
    public class AuditDbContextConfigurationManagerTests
    {
        public AuditDbContextConfigurationManagerTests()
        {
            AssemblyLoader.Current = new TestAssemblyLoader();
        }

        [Fact]
        public void ShouldGetAuditDbContextConfigurationFromXml()
        {
            // Act
            var auditDbContextConfiguration = AuditDbContextConfigurationManager.GetAuditDbContextConfigurationFromXml();

            // Assert
            auditDbContextConfiguration.Should().NotBeNull();
            auditDbContextConfiguration.AuditEnabled.Should().BeTrue();
            auditDbContextConfiguration.AuditDateTimeKind.Should().Be(DateTimeKind.Utc);
            auditDbContextConfiguration.AuditTypeInfos.Should().HaveCount(1);

            var auditTypeInfo = auditDbContextConfiguration.AuditTypeInfos.ElementAt(0);
            auditTypeInfo.AuditEntityType.Should().Be(typeof(EmployeeAudit));
            auditTypeInfo.AuditableEntityType.Should().Be(typeof(Employee));
            auditTypeInfo.AuditProperties.Should().BeEmpty();
        }

        [Fact]
        public void ShouldGetAuditDbContextConfigurationFromXml_ReturnsDefaultIfNonExisting()
        {
            // Arrange
            var configPath = Assembly.GetCallingAssembly().Location;

            // Act
            var auditDbContextConfiguration = AuditDbContextConfigurationManager.GetAuditDbContextConfigurationFromXml(configPath);

            // Assert
            auditDbContextConfiguration.Should().NotBeNull();
            auditDbContextConfiguration.AuditEnabled.Should().BeTrue();
            auditDbContextConfiguration.AuditDateTimeKind.Should().Be(DateTimeKind.Utc);
            auditDbContextConfiguration.AuditTypeInfos.Should().HaveCount(0);
        }
    }
}