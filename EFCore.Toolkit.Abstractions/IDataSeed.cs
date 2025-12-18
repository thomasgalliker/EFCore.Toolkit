using System.Linq.Expressions;

namespace EFCore.Toolkit.Abstractions
{
    /// <summary>
    /// Abstraction for a data seed.
    /// </summary>
    /// <remarks>There is a generic version of this interface available: <see cref="IDataSeed{TEntity}"/></remarks>
    public interface IDataSeed
    {
        /// <summary>
        /// Populates the specified <paramref name="context"/> with initial data required for application startup or testing.
        /// </summary>
        void Seed(IContext context);
    }

    /// <summary>
    /// Abstraction for a data seed for entities of type <typeparamref name="TEntity"/>.
    /// </summary>
    public interface IDataSeed<TEntity> : IDataSeed where TEntity : class
    {
        /// <summary>
        /// Gets the expression used to identify whether an entity should be added or updated in the data store.
        /// </summary>
        /// <remarks>The returned expression typically specifies the property or properties that uniquely
        /// identify an entity instance, such as a primary key. This expression is used in upsert operations to match
        /// entities in the data store.</remarks>
        public abstract Expression<Func<TEntity, object?>> AddOrUpdateExpression { get; }

        /// <summary>
        /// Returns all entities of type <typeparamref name="TEntity"/> which are used to seed the database.
        /// </summary>
        public IEnumerable<TEntity> GetAll();
    }
}