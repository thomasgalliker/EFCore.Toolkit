using EntityFramework.Toolkit.EFCore.Auditing;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToolkitSample.Model.Auditing;

namespace ToolkitSample.DataAccess.Context
{
    public class TestEntityAuditEntityTypeConfiguration : AuditEntityTypeConfiguration<TestEntityAudit, int>
    {
        public override void Configure(EntityTypeBuilder<TestEntityAudit> entity)
        {
            base.Configure(entity);

            entity.Property(e => e.TestEntityId).IsRequired();
        }
    }
}