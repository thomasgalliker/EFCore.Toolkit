using EFCore.Toolkit.Abstractions.Auditing;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Toolkit.Auditing
{
    public abstract class AuditEntityTypeConfiguration<TAuditEntity, TAuditKey> :
        EntityTypeConfiguration<TAuditEntity> where TAuditEntity : class,
        IAuditEntity<TAuditKey>
    {
        public override void Configure(EntityTypeBuilder<TAuditEntity> entity)
        {
            entity.HasKey(e => e.AuditId);
            entity.Property(e => e.AuditId).IsRequired();
            entity.Property(e => e.AuditDate).IsRequired();
            entity.Property(e => e.AuditUser).IsRequired();
            entity.Property(e => e.AuditType).IsRequired();
        }
    }
}