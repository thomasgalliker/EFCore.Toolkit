using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Abstractions.Extensions;

namespace EFCore.Toolkit
{
    /// <summary>
    /// The in-memory representation of <seealso cref="IGenericRepository{T}"/>.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class InMemoryRepository<T> : IGenericRepository<T> where T : IIdentifiable
    {
        private readonly List<T> items;

        public InMemoryRepository() : this(new List<T>())
        {
        }

        public InMemoryRepository(List<T> items)
        {
            this.items = items;
        }

        public void Dispose()
        {
            this.items.Clear();
        }

        /// <inheritdoc />
        public ChangeSet Save()
        {
            return ChangeSet.Empty;
        }

        /// <inheritdoc />
        public Task<ChangeSet> SaveAsync()
        {
            return Task.FromResult(ChangeSet.Empty);
        }

        /// <inheritdoc />
        public IQueryable<T> Get()
        {
            return this.items.AsQueryable();
        }

        /// <inheritdoc />
        public IEnumerable<T> GetAll()
        {
            return this.items;
        }

        /// <inheritdoc />
        public T FindById(params object[] ids)
        {
            var intIds = ids.Select(i => int.Parse($"{i}"));
            return this.items.SingleOrDefault(i => intIds.Contains(i.Id)); // TODO Test this implementation
        }

        /// <inheritdoc />
        public T Add(T entity)
        {
            entity.Id = this.items.GetNextId();
            this.items.Add(entity);

            return entity;
        }

        /// <inheritdoc />
        public IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            var collection = entities.ToList();
            foreach (var entity in collection)
            {
                this.Add(entity);
            }

            return collection;
        }

        /// <inheritdoc />
        public T AddOrUpdate(T entity)
        {
            this.Remove(entity);
            this.Add(entity);

            return entity;
        }

        /// <inheritdoc />
        public T Update(T entity)
        {
            this.items.Remove(entity);
            this.items.Add(entity);

            return entity;
        }

        /// <inheritdoc />
        public void UpdateRange(IEnumerable<T> entities)
        {
            this.RemoveRange(entities);
            this.AddRange(entities);
        }

        /// <inheritdoc />
        public T SetValues(T entity, T updateEntity)
        {
            this.items.Remove(entity);
            this.items.Add(updateEntity);

            return updateEntity;
        }

        /// <inheritdoc />
        public T UpdateProperties<TValue>(T entity, params Expression<Func<T, TValue>>[] propertyExpressions)
        {
            this.Remove(entity);
            this.Add(entity);

            return entity;
        }

        /// <inheritdoc />
        public T UpdateProperty<TValue>(T entity, Expression<Func<T, TValue>> propertyExpression, TValue value)
        {
            this.Remove(entity);
            this.Add(entity);

            return entity;
        }

        /// <inheritdoc />
        public T Remove(T entity)
        {
            this.items.Remove(entity);
            return entity;
        }

        /// <inheritdoc />
        public IEnumerable<T> RemoveRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                yield return this.Remove(entity);
            }
        }

        public IContext Context { get; }
    }
}