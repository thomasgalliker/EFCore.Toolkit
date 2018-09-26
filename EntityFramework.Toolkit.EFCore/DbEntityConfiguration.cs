using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFramework.Toolkit.EFCore
{
    // TODO: Rename to EntityTypeConfiguration
    public abstract class DbEntityConfiguration<TEntity> where TEntity : class
    {
        public abstract void Configure(EntityTypeBuilder<TEntity> entity);
    }
}