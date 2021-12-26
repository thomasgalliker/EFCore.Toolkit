using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Toolkit.Extensions
{
    public static class EntityTypeBuilderExtensions
    {
        /// <summary>
        /// Adds a column "RowVersion" of type byte[] used for optimistic concurrency detection.
        /// </summary>
        public static PropertyBuilder<byte[]> HasRowVersion<TProperty>(this EntityTypeBuilder entityTypeBuilder)
        {
            return entityTypeBuilder.Property<byte[]>("RowVersion")
                .IsRowVersion()
                .HasColumnName("RowVersion");
        }
    }
}
