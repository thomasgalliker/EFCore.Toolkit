using EntityFramework.Toolkit.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToolkitSample.Model.Auditing;

namespace ToolkitSample.DataAccess.Context
{
    public class TestEntityEntityTypeConfiguration : EntityTypeConfiguration<TestEntity>
    {
        public override void Configure(EntityTypeBuilder<TestEntity> entity)
        {
            entity.HasKey(e => e.TestEntityId);
        }
    }
}