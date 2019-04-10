using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EFCore.Toolkit.Auditing.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void AddConfiguration<TEntity>(this ModelBuilder modelBuilder, EntityTypeConfiguration<TEntity> entityConfiguration) where TEntity : class
        {
            modelBuilder.Entity<TEntity>(entityConfiguration.Configure);
        }

        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
        {
            modelBuilder.EntityTypes().Configure(et => et.Relational().TableName = et.DisplayName());
        }

        public static void RemoveCascadeDeleteConvention(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.EntityTypes())
            {
                entityType.GetForeignKeys()
                    .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade)
                    .ToList()
                    .ForEach(fk => fk.DeleteBehavior = DeleteBehavior.Restrict);
            }
        }

        public static IEnumerable<IMutableEntityType> EntityTypes(this ModelBuilder builder)
        {
            return builder.Model.GetEntityTypes();
        }

        public static IEnumerable<IMutableProperty> Properties(this ModelBuilder builder)
        {
            return builder.EntityTypes().SelectMany(entityType => entityType.GetProperties());
        }

        public static IEnumerable<IMutableProperty> Properties<T>(this ModelBuilder builder)
        {
            return builder.EntityTypes().SelectMany(entityType => entityType.GetProperties().Where(x => x.ClrType == typeof(T)));
        }

        public static void Configure(this IEnumerable<IMutableEntityType> entityTypes, Action<IMutableEntityType> convention)
        {
            foreach (var entityType in entityTypes)
            {
                convention(entityType);
            }
        }

        public static void Configure(this IEnumerable<IMutableProperty> propertyTypes, Action<IMutableProperty> convention)
        {
            foreach (var propertyType in propertyTypes)
            {
                convention(propertyType);
            }
        }
    }
}