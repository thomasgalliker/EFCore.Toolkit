using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EFCore.Toolkit.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void AddConfiguration<TEntity>(this ModelBuilder modelBuilder, EntityTypeConfiguration<TEntity> entityConfiguration) where TEntity : class
        {
            modelBuilder.Entity<TEntity>(entityConfiguration.Configure);
        }

#if !NETSTANDARD1_3 && !NETFX
        

        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
        {
            modelBuilder.EntityTypes().Configure(entityType =>
            {
                if (entityType.IsOwned() == false)
                {
                    var existingName = entityType.Relational().TableName;
                    var newName = entityType.DisplayName();
                    Console.WriteLine($"RemovePluralizingTableNameConvention: entityType '{entityType.Name}': {existingName} >>> {newName}");

                    entityType.Relational().TableName = entityType.DisplayName();
                }
            });
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
#endif

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
            return builder.EntityTypes().SelectMany(entityType => entityType.GetProperties().Where(p => p.ClrType == typeof(T)));
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

#if !NETSTANDARD1_3
        /// <summary>
        ///  Configures all decimals and nullable decimals with a default <paramref name="precision"/> (maximum allowed digits in a number) and <paramref name="scale"/> (maximum allowed digits after the decimal point).
        /// </summary>
        public static void SetDefaultDecimalPrecision(this ModelBuilder modelBuilder, int precision, int scale)
        {
            foreach (var p in modelBuilder.Properties<decimal>().Concat(modelBuilder.Properties<decimal?>()))
            {
                if (p.GetMaxLength() == null &&
                    p.IsConcurrencyToken == false &&
                    (p.Relational().ColumnType == null || p.Relational().ColumnType.StartsWith("decimal(", StringComparison.InvariantCultureIgnoreCase) == false))
                {
                    var columnType = $"decimal({precision},{scale})";
                    Console.WriteLine($"SetColumnType({columnType}): {p.DeclaringEntityType.Name}.{p.Name}");

                    p.Relational().ColumnType = $"decimal({precision},{scale})";
                }
            }
        }

        /// <summary>
        /// Configures all strings with a default <paramref name="maxLength"/>.
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <param name="maxLength"></param>
        /// <param name="includeEntityTypeFilter"></param>
        public static void SetDefaultStringMaxLength(this ModelBuilder modelBuilder, int? maxLength = 4000, Func<IMutableEntityType, bool> includeEntityTypeFilter = null)
        {
            foreach (var p in modelBuilder.Properties<string>())
            {
                if (p.GetMaxLength() == null &&
                    (includeEntityTypeFilter == null || includeEntityTypeFilter(p.DeclaringEntityType)) &&
                    p.IsConcurrencyToken == false)
                {
                    var maxLengthInternal = maxLength;
                    if (string.Equals(p.Relational().ColumnType, "nvarchar(MAX)", StringComparison.InvariantCultureIgnoreCase))
                    {
                        maxLengthInternal = null;
                    }

                    Console.WriteLine($"SetMaxLength({(maxLengthInternal == null ? "null" : $"{maxLengthInternal}")}): {p.DeclaringEntityType.Name}.{p.Name}");
                    p.SetMaxLength(maxLengthInternal);
                }
            }
        }
#endif
    }
}