using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Abstractions.Extensions;

namespace EFCore.Toolkit
{
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

        public ChangeSet Save()
        {
            return ChangeSet.Empty;
        }

        public Task<ChangeSet> SaveAsync()
        {
            return Task.FromResult(ChangeSet.Empty);
        }

        public IQueryable<T> Get()
        {
            return this.items.AsQueryable();
        }

        public IEnumerable<T> GetAll()
        {
            return this.items;
        }

        public T FindById(params object[] ids)
        {
            return this.items.FirstOrDefault(); // TODO Wrong implementation
        }

        public IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            return this.items;
        }

        public T Add(T entity)
        {
            entity.Id = this.items.GetNextId();
            this.items.Add(entity);

            return entity;
        }

        public IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            var collection = entities.ToList();
            foreach (var entity in collection)
            {
                this.Add(entity);
            }

            return collection;
        }

        public T AddOrUpdate(T entity)
        {
            this.Remove(entity);
            this.Add(entity);

            return entity;
        }

        public T Update(T entity)
        {
            this.items.Remove(entity);
            this.items.Add(entity);

            return entity;
        }

        public T Update(T entity, T updateEntity)
        {
            this.items.Remove(entity);
            this.items.Add(updateEntity);

            return updateEntity;
        }

        public T UpdateProperties<TValue>(T entity, params Expression<Func<T, TValue>>[] propertyExpressions)
        {
            this.Remove(entity);
            this.Add(entity);

            return entity;
        }

        public T UpdateProperty<TValue>(T entity, Expression<Func<T, TValue>> propertyExpression, TValue value)
        {
            this.Remove(entity);
            this.Add(entity);

            return entity;
        }

        public T Remove(T entity)
        {
            this.items.Remove(entity);

            return entity;
        }

        public void LoadReferenced<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> navigationProperty) where TEntity : class where TProperty : class
        {
        }

        public IContext Context { get; }
    }
}