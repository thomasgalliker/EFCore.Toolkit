using EFCore.Toolkit.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToolkitSample.Model.Auditing;

namespace ToolkitSample.DataAccess.Context
{
    public class EmployeeAuditEntityTypeConfiguration : AuditEntityTypeConfiguration<EmployeeAudit, int>
    {
        protected override void Configure(EntityTypeBuilder<EmployeeAudit> entity)
        {
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.LastName).IsRequired();
            entity.Property(e => e.FirstName).IsRequired();

            entity.ToTable(nameof(EmployeeAudit));
        }
    }
}