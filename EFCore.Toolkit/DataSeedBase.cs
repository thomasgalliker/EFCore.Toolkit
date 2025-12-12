using System.Linq.Expressions;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Extensions;

namespace EFCore.Toolkit
{
    /// <summary>
    /// Provides a template for generic seed implementors.
    /// </summary>
    /// <typeparam name="TEntity">The entity type for which the implementor provides a seed.</typeparam>
    public abstract class DataSeedBase<TEntity> : IDataSeed<TEntity> where TEntity : class
    {
        public abstract Expression<Func<TEntity, object>> AddOrUpdateExpression { get; }

        public abstract IEnumerable<TEntity> GetAll();

        public void Seed(IContext context)
        {
            var entities = this.GetAll().ToArray();
            context.AddOrUpdate(entities, this.AddOrUpdateExpression);
        }
    }
}