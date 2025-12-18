namespace EFCore.Toolkit.Auditing
{
    public class AuditDbContextConfiguration
    {
        public AuditDbContextConfiguration(AuditTypeInfo[] auditTypeInfos)
            : this(auditTypeInfos, DateTimeKind.Utc)
        {
        }

        public AuditDbContextConfiguration(AuditTypeInfo[] auditTypeInfos, DateTimeKind auditDateTimeKind = DateTimeKind.Utc)
        {
            this.AuditDateTimeKind = auditDateTimeKind;
            this.AuditTypeInfos = auditTypeInfos;
        }

        public AuditTypeInfo[] AuditTypeInfos { get; }

        public DateTimeKind AuditDateTimeKind { get; }
    }
}