using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFramework.Toolkit.EFCore
{
    public abstract class EntityTypeConfiguration<TEntity> where TEntity : class
    {
        public abstract void Configure(EntityTypeBuilder<TEntity> entity);
    }
}