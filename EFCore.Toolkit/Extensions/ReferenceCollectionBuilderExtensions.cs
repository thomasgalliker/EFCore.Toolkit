using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Toolkit.Extensions
{
    public static class ReferenceCollectionBuilderExtensions
    {
        /// <summary>
        /// Configures whether this is a optional relationship (i.e. whether the foreign key property(s) can
        /// be assigned <see langword="null" />).
        /// </summary>
        /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
        public static ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> IsOptional<TPrincipalEntity, TDependentEntity>(this ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> builder)
            where TPrincipalEntity : class
            where TDependentEntity : class
        {
            return builder.IsRequired(required: false);
        }
    }
}