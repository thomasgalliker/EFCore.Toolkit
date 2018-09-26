using EntityFramework.Toolkit.EFCore.Contracts.Auditing;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFramework.Toolkit.EFCore.Auditing
{
    public abstract class AuditEntityTypeConfiguration<TAuditEntity, TAuditKey> :
        DbEntityConfiguration<TAuditEntity> where TAuditEntity : class,
        IAuditEntity<TAuditKey>
    {
        public override void Configure(EntityTypeBuilder<TAuditEntity> entity)
        {
            entity.HasKey(e => e.AuditId);
            entity.Property(e => e.AuditDate).IsRequired();
            entity.Property(e => e.AuditUser).IsRequired();
            entity.Property(e => e.AuditType).IsRequired();
        }
    }
}