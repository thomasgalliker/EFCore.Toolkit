using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Toolkit.Extensions
{
    public static class PropertyBuilderExtensions
    {
        /// <summary>
        /// Marks the property as optional (IsRequired(required: false)).
        /// </summary>
        public static PropertyBuilder<TProperty> IsOptional<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
        {
            return propertyBuilder.IsRequired(required: false);
        }

        /// <summary>
        /// Configures the maximum length of data that can be stored in this property.
        /// </summary>
        public static PropertyBuilder<string> HasMaxLength(this PropertyBuilder<string> propertyBuilder, int? maxLength = null)
        {
            if (maxLength is int value)
            {
                return propertyBuilder.HasMaxLength(value);
            }
            else
            {
                return propertyBuilder.HasColumnType("nvarchar(MAX)");
            }
        }
    }
}
