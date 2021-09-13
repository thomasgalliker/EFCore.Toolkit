using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToolkitSample.Model;

namespace ToolkitSample.DataAccess.Context
{
    public class DepartmentEntityConfiguration : IEntityTypeConfiguration<Model.Department>
    {

        public void Configure(EntityTypeBuilder<Department> entity)
        {
            entity.HasKey(d => d.Id);

            entity.Property(d => d.Name).IsRequired();
            entity.Property(d => d.Name).HasMaxLength(255);
            entity.Property(d => d.Name)
                //TODO .IsUnique()
                ;

            ////entity.HasMany(d => d.Employees)
            ////    .WithOptional(e => e.Department);

            entity.HasOne(d => d.Leader)
                .WithMany()
                .HasForeignKey(d => d.LeaderId);

            //entity.HasRequired(d => d.Leader)
            //    .WithMany()
            //    .HasForeignKey(d => d.LeaderId);

            //entity.HasOptional(d => d.Leader);

            entity.Property(e => e.RowVersion)
                .ValueGeneratedOnAddOrUpdate()
                //.HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed)
                .HasMaxLength(8)
                .IsRowVersion()
                .IsRequired();

        }

    }
}