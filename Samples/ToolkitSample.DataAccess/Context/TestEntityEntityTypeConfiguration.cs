using EFCore.Toolkit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToolkitSample.Model.Auditing;

namespace ToolkitSample.DataAccess.Context
{
    public class TestEntityEntityTypeConfiguration : IEntityTypeConfiguration<TestEntity>
    {
        public void Configure(EntityTypeBuilder<TestEntity> entity)
        {
            entity.HasKey(e => e.TestEntityId);
        }
    }
}