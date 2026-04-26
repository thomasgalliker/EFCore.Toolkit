using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToolkitSample.Model;

namespace ToolkitSample.DataAccess.Context
{
    public class CountryEntityTypeConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> entity)
        {
            entity.HasKey(e => e.Id);

            entity.Property(t => t.Id)
                .ValueGeneratedNever()
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(255)
                //TODO .IsUnique()
                ;
        }
    }
}