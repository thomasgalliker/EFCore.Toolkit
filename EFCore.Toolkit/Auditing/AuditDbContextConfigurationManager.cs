#if !NETSTANDARD1_3
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using EFCore.Toolkit.Auditing.ConfigFile;

namespace EFCore.Toolkit.Auditing
{
    internal static class AuditDbContextConfigurationManager
    {
        internal static AuditDbContextConfiguration GetAuditDbContextConfigurationFromXml()
        {
            string configPath;
            var entryAssemblyLocation = Assembly.GetEntryAssembly().Location;
            if (entryAssemblyLocation.EndsWith("testhost.dll", StringComparison.InvariantCultureIgnoreCase))
            {
                configPath = "EFCore.Toolkit.Tests.dll";
            }
            else
            {
                configPath = Assembly.GetExecutingAssembly().Location;
            }

            return GetAuditDbContextConfigurationFromXml(configPath);
        }

        internal static AuditDbContextConfiguration GetAuditDbContextConfigurationFromXml(string configPath)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(configPath);
            var auditConfigurationSection = configuration.Sections["entityFramework.Audit"] as AuditConfigurationSection;
            auditConfigurationSection = auditConfigurationSection ?? new AuditConfigurationSection();

            var entityMapping = new List<AuditTypeInfo>();

            foreach (EntityElement entityElement in auditConfigurationSection.Entities)
            {
                var auditableEntityType = Type.GetType(entityElement.EntityType);
                if (auditableEntityType == null)
                {
                    throw new InvalidOperationException($"Auditable entity type '{entityElement.EntityType}' could not be loaded.");
                }

                var auditEntityType = Type.GetType(entityElement.AuditEntityType);
                if (auditEntityType == null)
                {
                    throw new InvalidOperationException($"Audit entity type '{entityElement.AuditEntityType}' could not be loaded.");
                }

                var auditTypeInfo = new AuditTypeInfo(auditableEntityType, auditEntityType);
                entityMapping.Add(auditTypeInfo);
            }

            return new AuditDbContextConfiguration(auditConfigurationSection.AuditEnabled, auditConfigurationSection.AuditDateTimeKind, entityMapping.ToArray());
        }
    }
}
#endif