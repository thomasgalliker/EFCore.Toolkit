using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToolkitSample.Model;
using EFCore.Toolkit.Extensions;

namespace ToolkitSample.DataAccess.Context
{
    public class PersonEntityTypeConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> entity)
        {
            entity.HasId();

            entity.Property(e => e.LastName).IsRequired().HasMaxLength(255);

            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(255);

            entity.Property(e => e.Birthdate).IsRequired();

            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.UpdatedDate).IsRequired(false);

            entity.HasOne(t => t.Country)
                .WithMany()
                .HasForeignKey(d => d.CountryId)
                .IsRequired(false);

            entity.Property(e => e.RowVersion)
                .ValueGeneratedOnAddOrUpdate()
                .HasMaxLength(8)
                .IsRowVersion()
                .IsRequired();

            entity.HasBaseType((Type?)null);
        }
    }
}