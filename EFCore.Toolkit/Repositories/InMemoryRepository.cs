using System.Linq.Expressions;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Extensions;
using EFCore.Toolkit.Testing;

namespace EFCore.Toolkit
{
    /// <summary>
    /// The in-memory representation of <seealso cref="IGenericRepository{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class InMemoryRepository<TEntity> : IGenericRepository<TEntity> where TEntity : IIdentifiable
    {
        private readonly List<TEntity> items;

        public InMemoryRepository()
            : this(new List<TEntity>())
        {
        }

        public InMemoryRepository(List<TEntity> items)
        {
            this.items = items;
        }

        public void Dispose()
        {
            lock (this.items)
            {
                this.items.Clear();
            }
        }

        /// <inheritdoc />
        public ChangeSet Save()
        {
            return new ChangeSet(this.GetType(), Array.Empty<IChange>());
        }

        /// <inheritdoc />
        public Task<ChangeSet> SaveAsync()
        {
            var changeSet = this.Save();
            return Task.FromResult(changeSet);
        }

        /// <inheritdoc />
        public IQueryable<TEntity> Get()
        {
            lock (this.items)
            {
                return new TestAsyncEnumerable<TEntity>(this.items);
            }
        }

        /// <inheritdoc />
        public IEnumerable<TEntity> GetAll()
        {
            lock (this.items)
            {
                return this.items;
            }
        }

        /// <inheritdoc />
        public TEntity? FindById(params object[] ids)
        {
            var intIds = ids.Select(i => int.Parse($"{i}"));

            lock (this.items)
            {
                return this.items.SingleOrDefault(i => intIds.Contains(i.Id)); // TODO Test this implementation
            }
        }

        /// <inheritdoc />
        public TEntity Add(TEntity entity)
        {
            lock (this.items)
            {
                entity.Id = this.items.GetNextId();
                this.items.Add(entity);
            }

            return entity;
        }

        /// <inheritdoc />
        public IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities)
        {
            var collection = entities.ToList();
            foreach (var entity in collection)
            {
                this.Add(entity);
            }

            return collection;
        }

        /// <inheritdoc />
        public TEntity AddOrUpdate(TEntity entity)
        {
            lock (this.items)
            {
                this.items.Remove(entity);
                this.items.Add(entity);
            }

            return entity;
        }

        /// <inheritdoc />
        public TEntity Update(TEntity entity)
        {
            lock (this.items)
            {
                this.items.Remove(entity);
                this.items.Add(entity);
            }

            return entity;
        }

        /// <inheritdoc />
        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            lock (this.items)
            {
                this.RemoveRange(entities);
                this.AddRange(entities);
            }
        }

        /// <inheritdoc />
        public TEntity SetValues(TEntity entity, TEntity updateEntity)
        {
            lock (this.items)
            {
                this.items.Remove(entity);
                this.items.Add(updateEntity);
            }

            return updateEntity;
        }

        /// <inheritdoc />
        public TEntity UpdateProperties<TValue>(TEntity entity, params Expression<Func<TEntity, TValue>>[] propertyExpressions)
        {
            this.Remove(entity);
            this.Add(entity);

            return entity;
        }

        /// <inheritdoc />
        public TEntity UpdateProperty<TValue>(TEntity entity, Expression<Func<TEntity, TValue>> propertyExpression, TValue? value)
        {
            this.Remove(entity);
            this.Add(entity);

            return entity;
        }

        /// <inheritdoc />
        public TEntity Remove(TEntity entity)
        {
            lock (this.items)
            {
                this.items.Remove(entity);
            }

            return entity;
        }

        /// <inheritdoc />
        public IEnumerable<TEntity> RemoveRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                yield return this.Remove(entity);
            }
        }

        public IContext Context
        {
            get
            {
                throw new NotSupportedException();
            }
        }
    }
}