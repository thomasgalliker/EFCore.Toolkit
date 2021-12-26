using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace EFCore.Toolkit.Extensions
{
    public static class DbContextOptionsExtensions
    {
        private static DbContextOptions ContextOptions;

        public static void ConfigureDefaultValue(this DbContextOptions contextOptions)
        {
            ContextOptions = contextOptions;
        }

        private static readonly Dictionary<string, string> NewIdDictionary = new Dictionary<string, string>
        {
            { nameof(SqlServerOptionsExtension), "NEWID()" },
        };

        public static PropertyBuilder<Guid> HasDefaultValueForSql<Guid>(this PropertyBuilder<Guid> propertyBuilder)
        {
            if (ContextOptions == null)
            {
                return propertyBuilder;
            }

            var defaultValues = ContextOptions.Extensions
                .Where(e => e is RelationalOptionsExtension)
                .Select(extension =>
                {
                    var extensionType = extension.GetType();

                    string newIdValue = null;
                    if (NewIdDictionary.TryGetValue(extensionType.Name, out var sqlNewIdValue))
                    {
                        newIdValue = sqlNewIdValue;
                    }

                    Console.WriteLine($"HasDefaultValueForSql extension={extensionType.Name} -> newIdValue=\"{ newIdValue ?? "<null>"}\"");
                    return newIdValue;
                }).Where(s => !string.IsNullOrEmpty(s));

            var defaultValue = defaultValues.SingleOrDefault();
            if (defaultValue != null)
            {
                return propertyBuilder.HasDefaultValueSql(defaultValue);
            }
            else
            {
                return propertyBuilder.HasValueGenerator((property, type) => new GuidValueGenerator());
            }
        }
    }
}