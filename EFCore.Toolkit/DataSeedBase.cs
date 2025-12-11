using System.Linq.Expressions;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Extensions;

namespace EFCore.Toolkit
{
    /// <summary>
    ///     Provides a template for generic seed implementors.
    /// </summary>
    /// <typeparam name="TEntity">The entity type for which the implementor provides a seed.</typeparam>
    public abstract class DataSeedBase<TEntity> : IDataSeed where TEntity : class
    {
        public abstract Expression<Func<TEntity, object>> AddOrUpdateExpression { get; }

        public abstract TEntity[] GetAll();

        public void Seed(IContext context)
        {
            context.AddOrUpdate(this.GetAll(), this.AddOrUpdateExpression);
        }
    }
}