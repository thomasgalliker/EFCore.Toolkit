using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.Toolkit.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
        {
            modelBuilder.EntityTypes().Configure(entityType =>
            {
                if (entityType.IsOwned() == false)
                {
                    var existingTableName = entityType.GetTableName();
                    var newTableName = entityType.DisplayName();
                    Console.WriteLine($"RemovePluralizingTableNameConvention: entityType '{entityType.Name}': {existingTableName} >>> {newTableName}");

                    entityType.SetTableName(entityType.DisplayName());
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

        /// <summary>
        ///  Configures all decimals and nullable decimals with a default <paramref name="precision"/> (maximum allowed digits in a number) and <paramref name="scale"/> (maximum allowed digits after the decimal point).
        /// </summary>
        public static void SetDefaultDecimalPrecision(this ModelBuilder modelBuilder, int precision, int scale)
        {
            foreach (var p in modelBuilder.Properties<decimal>().Concat(modelBuilder.Properties<decimal?>()))
            {
                var originalColumnType = p.GetColumnType();
                if (p.GetMaxLength() == null &&
                    p.IsConcurrencyToken == false &&
                    (originalColumnType == null || originalColumnType.StartsWith("decimal(", StringComparison.InvariantCultureIgnoreCase) == false))
                {
                    var newColumnType = $"decimal({precision},{scale})";
                    Console.WriteLine($"SetColumnType({newColumnType}): {p.DeclaringEntityType.Name}.{p.Name}");

                    p.SetColumnType(newColumnType);
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
                    var originalColumnType = p.GetColumnType();
                    var maxLengthInternal = maxLength;
                    if (string.Equals(originalColumnType, "nvarchar(MAX)", StringComparison.InvariantCultureIgnoreCase))
                    {
                        maxLengthInternal = null;
                    }

                    Console.WriteLine($"SetMaxLength({(maxLengthInternal == null ? "null" : $"{maxLengthInternal}")}): {p.DeclaringEntityType.Name}.{p.Name}");
                    p.SetMaxLength(maxLengthInternal);
                }
            }
        }
    }
}