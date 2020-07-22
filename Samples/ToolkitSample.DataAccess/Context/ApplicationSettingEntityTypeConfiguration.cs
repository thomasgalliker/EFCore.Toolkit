using EFCore.Toolkit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToolkitSample.Model;

namespace ToolkitSample.DataAccess.Context
{
    public class ApplicationSettingEntityTypeConfiguration : IEntityTypeConfiguration<ApplicationSetting>
    {
        public void Configure(EntityTypeBuilder<ApplicationSetting> entity)
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).ValueGeneratedNever();
            entity.Property(d => d.Path).HasMaxLength(255);
        }
    }
}