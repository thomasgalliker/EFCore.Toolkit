using EFCore.Toolkit.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToolkitSample.Model;

namespace ToolkitSample.DataAccess.Context
{
    public class DepartmentEntityConfiguration : IEntityTypeConfiguration<Department>
    {

        public void Configure(EntityTypeBuilder<Department> entity)
        {
            entity.HasId();

            entity.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(d => d.Description)
                .IsOptional();

            entity.HasOne(d => d.Leader)
                .WithMany()
                .HasForeignKey(d => d.LeaderId);

            entity.Property(e => e.RowVersion)
                .IsRowVersion();
        }
    }
}