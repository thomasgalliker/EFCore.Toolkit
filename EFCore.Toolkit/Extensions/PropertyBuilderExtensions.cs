using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Toolkit.Abstractions.Extensions
{
    public static class PropertyBuilderExtensions
    {
        public static PropertyBuilder<TProperty> IsOptional<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
        {
            return propertyBuilder.IsRequired(required: false);
        }

        public static PropertyBuilder<string> HasMaxLength(this PropertyBuilder<string> propertyBuilder)
        {
            return propertyBuilder.HasColumnType("nvarchar(MAX)");
        }
    }
}
