using EFCore.Toolkit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToolkitSample.Model;

namespace ToolkitSample.DataAccess.Context
{
    public class RoomEntityTypeConfiguration : EntityTypeConfiguration<Room>
    {
        public override void Configure(EntityTypeBuilder<Room> entity)
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Description).IsRequired(false)
                // TODO PropertyBuilderExtensions with .IsOptional()
                .HasMaxLength(255);

            entity.Property(e => e.Level);
            entity.Property(e => e.Sector).HasMaxLength(900);

            entity.HasIndex(e => new { e.Level, e.Sector }).IsUnique();

            entity.ToTable(nameof(Room));
        }
    }
}