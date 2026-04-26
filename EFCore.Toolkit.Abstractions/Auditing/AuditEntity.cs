namespace EFCore.Toolkit.Abstractions.Auditing
{
    /// <summary>
    /// Base class for audit entities.
    /// </summary>
    public abstract class AuditEntity : IAuditEntity<int>
    {
        public int AuditId { get; set; }

        /// <summary>
        /// Gets or sets the DateTime this audit entity was created.
        /// Will be automatically set by AuditDbContext on SaveChanges.
        /// </summary>
        public DateTime AuditDate { get; set; }

        /// <summary>
        /// Gets or sets the user who updated the entity
        /// Will be automatically set by AuditDbContext on SaveChanges.
        /// </summary>
        public string? AuditUser { get; set; }

        /// <summary>
        /// Gets or sets the type of audit. 0 for update, 1 for deletion.
        /// Will be automatically set by AuditDbContext on SaveChanges.
        /// </summary>
        public AuditEntityState AuditType { get; set; }
    }
}