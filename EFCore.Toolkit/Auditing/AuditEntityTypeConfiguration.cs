using EFCore.Toolkit.Abstractions.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Toolkit.Auditing
{
    public abstract class AuditEntityTypeConfiguration<TAuditEntity, TAuditKey> :
        IEntityTypeConfiguration<TAuditEntity> where TAuditEntity : class,
        IAuditEntity<TAuditKey>
    {
        protected virtual void Configure(EntityTypeBuilder<TAuditEntity> entity)
        {
        }

        void IEntityTypeConfiguration<TAuditEntity>.Configure(EntityTypeBuilder<TAuditEntity> entity)
        {
            this.Configure(entity);

            entity.HasKey(e => e.AuditId);
            entity.Property(e => e.AuditId).IsRequired();
            entity.Property(e => e.AuditDate).IsRequired();
            entity.Property(e => e.AuditUser).IsRequired();
            entity.Property(e => e.AuditType).IsRequired();
        }
    }
}