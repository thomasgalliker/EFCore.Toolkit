using EFCore.Toolkit.Auditing;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToolkitSample.Model.Auditing;

namespace ToolkitSample.DataAccess.Context
{
    public class TestEntityAuditEntityTypeConfiguration : AuditEntityTypeConfiguration<TestEntityAudit, int>
    {
        public void Configure(EntityTypeBuilder<TestEntityAudit> entity)
        {
            base.Configure(entity);

            entity.HasKey(e => e.TestEntityId);
        }
    }
}