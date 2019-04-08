using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using EFCore.Toolkit.Abstractions.Auditing;

namespace EFCore.Toolkit.Auditing
{
    public class AuditTypeInfo
    {
        /// <summary>
        ///     Initializes a new instance of the AuditTypeInfo class.
        ///     <param name="auditableEntityType">Type to audit, must implement IAuditableEntity.</param>
        ///     <param name="auditEntityType">Type of audit entity, must implement IAuditEntity.</param>
        /// </summary>
        public AuditTypeInfo(Type auditableEntityType, Type auditEntityType)
        {
            if (auditableEntityType == null)
            {
                throw new ArgumentNullException(nameof(auditableEntityType));
            }

            if (auditEntityType == null)
            {
                throw new ArgumentNullException(nameof(auditEntityType));
            }

            var auditEntityInterface = auditEntityType.GetTypeInfo().ImplementedInterfaces.SingleOrDefault(i => i.Name == nameof(IAuditEntity));
            if (auditEntityInterface == null)
            {
                throw new ArgumentException($"Entity of type {auditEntityType.Name} does implement {nameof(IAuditEntity)}.",
                    nameof(auditEntityType));
            }

            this.AuditableEntityType = auditableEntityType;
            this.AuditEntityType = auditEntityType;
            this.AuditProperties = new Collection<string>();
        }

        public Type AuditableEntityType { get; private set; }

        public Type AuditEntityType { get; private set; }

        public Collection<string> AuditProperties { get; private set; }
    }
}