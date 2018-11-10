using EFCore.Toolkit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToolkitSample.Model;

namespace ToolkitSample.DataAccess.Context
{
    public class StudentEntityConfiguration : EntityTypeConfiguration<Student>
    {
        public override void Configure(EntityTypeBuilder<Student> entity)
        {
            entity.Property(e => e.EnrollmentDate).IsRequired();

            entity.ToTable(nameof(Student));
        }
    }
}