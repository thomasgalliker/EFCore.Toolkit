namespace EFCore.Toolkit.Abstractions.Extensions
{
    public static class DeletableExtensions
    {
        /// <summary>
        /// Checks if the given <paramref name="entity"/> has a property <c>IsDeleted</c>
        /// and the property returns <c>true</c>.
        /// </summary>
        /// <typeparam name="TEntity">The entity.</typeparam>
        public static bool IsDeleted<TEntity>(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var entityType = typeof(TEntity);
            var propertyInfo = entityType.GetProperties().FirstOrDefault(c => c.Name == nameof(IDeletable.IsDeleted));
            if (propertyInfo == null)
            {
                return false;
            }

            var isDeleted = (bool)propertyInfo.GetValue(entity);
            return isDeleted;
        }
    }
}