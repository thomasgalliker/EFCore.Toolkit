using System.Diagnostics;
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
                    Debug.WriteLine($"RemovePluralizingTableNameConvention: entityType '{entityType.Name}': {existingTableName} >>> {newTableName}");

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
        /// Applies a default precision and scale to all decimal properties that do not already have them configured.
        /// Default precision is 18 and scale is 2.
        /// </summary>
        public static void SetDefaultDecimalPrecision(this ModelBuilder modelBuilder, int precision = 18, int scale = 2)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var p in entityType.GetProperties())
                {
                    if (p.ClrType != typeof(decimal) && p.ClrType != typeof(decimal?))
                    {
                        continue;
                    }

                    if (p.GetPrecision() != null || p.GetScale() != null)
                    {
                        continue;
                    }

                    if (p.IsKey() ||
                        p.IsForeignKey() ||
                        p.IsConcurrencyToken ||
                        p.IsShadowProperty())
                    {
                        continue;
                    }

                    Debug.WriteLine($"{entityType.Name}.{p.Name} >> SetPrecision({precision},{scale})");
                    p.SetPrecision(precision);
                    p.SetScale(scale);
                }
            }
        }

        /// <summary>
        /// Applies a default maximum length to all string properties that do not already have one configured,
        /// preventing unbounded string columns (e.g. varchar(max)).
        /// </summary>
        public static void SetDefaultStringMaxLength(this ModelBuilder modelBuilder, int maxLength = 4000, Func<IMutableTypeBase, bool>? filter = null)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var p in entityType.GetProperties())
                {
                    if (p.ClrType != typeof(string))
                    {
                        continue;
                    }

                    if (p.GetMaxLength() != null)
                    {
                        continue;
                    }

                    if (p.IsConcurrencyToken ||
                        p.IsKey() ||
                        p.IsForeignKey() ||
                         p.IsShadowProperty() ||
                         (p.DeclaringType is IMutableEntityType et && et.GetDiscriminatorPropertyName() == p.Name))
                    {
                        continue;
                    }

                    if (filter != null && !filter(p.DeclaringType))
                    {
                        continue;
                    }

                    Debug.WriteLine($"{entityType.Name}.{p.Name} >> SetMaxLength({maxLength})");
                    p.SetMaxLength(maxLength);
                }
            }
        }

    }
}