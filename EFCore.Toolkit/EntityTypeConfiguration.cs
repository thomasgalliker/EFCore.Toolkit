using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Toolkit
{
    public abstract class EntityTypeConfiguration<TEntity> where TEntity : class
    {
        public abstract void Configure(EntityTypeBuilder<TEntity> entity);
    }
}