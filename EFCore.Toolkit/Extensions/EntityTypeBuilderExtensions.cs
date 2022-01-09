using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Abstractions.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Toolkit.Extensions
{
    public static class EntityTypeBuilderExtensions
    {
        /// <summary>
        /// Configures an entity as <seealso cref="IIdentifiable"/>.
        /// </summary>
        /// <typeparam name="TEntity">Entity which implements <seealso cref="IIdentifiable"/>.</typeparam>
        /// <param name="entity">EntityTypeBuilder for <typeparamref name="TEntity"/>.</param>
        public static EntityTypeBuilder<TEntity> HasId<TEntity>(this EntityTypeBuilder<TEntity> entity) where TEntity : class, IIdentifiable
        {
            entity.HasKey(e => e.Id).IsClustered(true);

            return entity;
        }

        /// <summary>
        /// Configures an entity as <seealso cref="IExternalIdentifiable"/>.
        /// </summary>
        /// <typeparam name="TEntity">Entity which implements <seealso cref="IExternalIdentifiable"/>.</typeparam>
        /// <param name="entity">EntityTypeBuilder for <typeparamref name="TEntity"/>.</param>
        public static EntityTypeBuilder<TEntity> HasExternalId<TEntity>(this EntityTypeBuilder<TEntity> entity) where TEntity : class, IExternalIdentifiable
        {
            entity.HasIndex(e => e.ExternalId).IsUnique().IsClustered(false);
            entity.Property(e => e.ExternalId).ValueGeneratedOnAdd().HasDefaultValueForSql();

            return entity;
        }

        /// <summary>
        /// Configures an entity as <seealso cref="ICreatedBy{TKey}"/>.
        /// </summary>
        /// <typeparam name="TEntity">Entity which implements <seealso cref="ICreatedBy{TKey}"/>.</typeparam>
        /// <param name="entity">EntityTypeBuilder for <typeparamref name="TEntity"/>.</param>
        public static EntityTypeBuilder<TEntity> HasCreatedBy<TEntity, TKey>(this EntityTypeBuilder<TEntity> entity) where TEntity : class, ICreatedBy<TKey>
        {
            entity.HasIndex(e => e.CreatedBy);
            entity.Property(e => e.CreatedBy).IsRequired();

            return entity;
        }

        /// <summary>
        /// Configures an entity as <seealso cref="ICreatedBy{int}"/>.
        /// </summary>
        /// <typeparam name="TEntity">Entity which implements <seealso cref="ICreatedBy{int}"/>.</typeparam>
        /// <param name="entity">EntityTypeBuilder for <typeparamref name="TEntity"/>.</param>
        public static EntityTypeBuilder<TEntity> HasCreatedBy<TEntity>(this EntityTypeBuilder<TEntity> entity) where TEntity : class, ICreatedBy<int>
        {
            return entity.HasCreatedBy<TEntity, int>();
        }

        /// <summary>
        /// Configures an entity as <seealso cref="ICreatedDate"/>.
        /// </summary>
        /// <typeparam name="TEntity">Entity which implements <seealso cref="ICreatedDate"/>.</typeparam>
        /// <param name="entity">EntityTypeBuilder for <typeparamref name="TEntity"/>.</param>
        public static EntityTypeBuilder<TEntity> HasCreatedDate<TEntity>(this EntityTypeBuilder<TEntity> entity) where TEntity : class, ICreatedDate
        {
            entity.Property(e => e.CreatedDate).IsRequired();

            return entity;
        }

        /// <summary>
        /// Configures an entity as <seealso cref="IUpdatedDate"/>.
        /// </summary>
        /// <typeparam name="TEntity">Entity which implements <seealso cref="IUpdatedDate"/>.</typeparam>
        /// <param name="entity">EntityTypeBuilder for <typeparamref name="TEntity"/>.</param>
        public static EntityTypeBuilder<TEntity> HasUpdatedDate<TEntity>(this EntityTypeBuilder<TEntity> entity) where TEntity : class, IUpdatedDate
        {
            entity.Property(e => e.UpdatedDate).IsOptional();

            return entity;
        }

        /// <summary>
        /// Configures an entity as <seealso cref="IDeletable"/>.
        /// </summary>
        /// <typeparam name="TEntity">Entity which implements <seealso cref="IDeletable"/>.</typeparam>
        /// <param name="entity">EntityTypeBuilder for <typeparamref name="TEntity"/>.</param>
        public static PropertyBuilder<bool> IsDeletable<TEntity>(this EntityTypeBuilder<TEntity> entity) where TEntity : class, IDeletable
        {
            return entity.Property(u => u.IsDeleted).IsRequired();
        }

        /// <summary>
        /// Adds a column "RowVersion" of type byte[] used for optimistic concurrency detection.
        /// </summary>
        public static PropertyBuilder<byte[]> HasRowVersion(this EntityTypeBuilder entityTypeBuilder)
        {
            return entityTypeBuilder.Property<byte[]>("RowVersion")
                .IsRowVersion()
                .HasColumnName("RowVersion");
        }
    }
}
