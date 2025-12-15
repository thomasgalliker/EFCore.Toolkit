using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToolkitSample.Model;
using EFCore.Toolkit.Extensions;

namespace ToolkitSample.DataAccess.Context
{
    public class EmployeeEntityTypeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> entity)
        {
            entity.HasBaseType<Person>();

            entity.Property(e => e.EmployementDate).IsOptional();

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .IsRequired(false);

            entity.Property(e => e.PropertyA);
            entity.Property(e => e.PropertyB);

            //this.Unique(e => e.PropertyA, e => e.PropertyB);
        }
    }
}