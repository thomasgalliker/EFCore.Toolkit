using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Extensions;
using EFCore.Toolkit.Utils;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EFCore.Toolkit
{
    public class GenericRepository<TEntity, TUserKey> : GenericRepository<TEntity>, IUserContextAwareRepository<TEntity> where TEntity : class, ICreatedBy<TUserKey>
    {
        private readonly IUserContext<TUserKey> userContext;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GenericRepository{TEntity, TUserKey}" /> class.
        /// </summary>
        public GenericRepository(IDbContext context, IUserContext<TUserKey> userContext) : base(context)
        {
            this.userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        /// <summary>
        /// Returns <see cref="IQueryable{TEntity}"/> which filters entities by current user.
        /// </summary>
        public override IQueryable<TEntity> Get()
        {
            return this.Get(filterByCurrentUser: true);
        }

        /// <summary>
        /// Returns <see cref="IQueryable{TEntity}"/> which allows to control whether or not to filter entities by current user.
        /// </summary>
        /// <param name="filterByCurrentUser">Returns current user's entities if <c>true</c>. No filter applied if <c>false</c>.</param>
        public IQueryable<TEntity> Get(bool filterByCurrentUser)
        {
            if (filterByCurrentUser)
            {
                var currentUserId = this.userContext.GetCurrentUserId();
                return base.Get().Where(i => Equals(i.CreatedBy, currentUserId));
            }

            return base.Get();
        }
    }

    /// <summary>
    ///     Implementation of a generic repository.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericRepository<T> : IGenericRepository<T>
        where T : class
    {
        protected readonly DbSet<T> DbSet;
        private readonly IDbContext context;

        private bool isDisposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GenericRepository{T}" /> class.
        /// </summary>
        public GenericRepository(IDbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));

            this.DbSet = this.context.Set<T>();
        }

        /// <inheritdoc />
        public IContext Context => this.context;

        /// <inheritdoc />
        public virtual IQueryable<T> Get()
        {
            return this.DbSet;
        }

        /// <inheritdoc />
        public virtual IEnumerable<T> GetAll()
        {
            return this.Get().AsEnumerable();
        }

        /// <inheritdoc />
        public T FindById(params object[] ids)
        {
            return this.DbSet.Find(ids);
        }

        /// <inheritdoc />
        public virtual T Add(T entity)
        {
            return this.DbSet.Add(entity).Entity;
        }

        /// <inheritdoc />
        public virtual IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            this.DbSet.AddRange(entities);
            return entities;
        }

        /// <inheritdoc />
        public virtual T AddOrUpdate(T entity)
        {
            return ((DbContext)this.context).AddOrUpdate(entity);
        }

        /// <inheritdoc />
        public virtual T Update(T entity)
        {
            return this.DbSet.Update(entity).Entity;
        }

        /// <inheritdoc />
        public void UpdateRange(IEnumerable<T> entities)
        {
            this.DbSet.UpdateRange(entities);
        }

        /// <inheritdoc />
        public virtual T SetValues(T entity, T updateEntity)
        {
            return this.context.SetValues(entity, updateEntity);
        }

        /// <inheritdoc />
        public virtual T UpdateProperties<TValue>(T entity, params Expression<Func<T, TValue>>[] propertyExpressions)
        {
            this.context.SetStateUnchanged(entity);

            var propertyNames = propertyExpressions.Select(pe => pe.GetPropertyInfo().Name).ToArray();
            this.context.ModifyProperties(entity, propertyNames);

            return entity;
        }

        /// <inheritdoc />
        public virtual T UpdateProperty<TValue>(T entity, Expression<Func<T, TValue>> propertyExpression, TValue value)
        {
            entity = this.UpdateProperties(entity, propertyExpression);

            entity.SetPropertyValue(propertyExpression.GetPropertyInfo().Name, value);
            return entity;
        }

        /// <inheritdoc />
        public virtual T Remove(T entity)
        {
            return this.DbSet.Remove(entity).Entity;
        }

        /// <inheritdoc />
        public virtual IEnumerable<T> RemoveRange(IEnumerable<T> entities)
        {
            this.DbSet.RemoveRange(entities);
            return entities;
        }

        /// <inheritdoc />
        public virtual ChangeSet Save()
        {
            return this.context.SaveChanges();
        }

        /// <inheritdoc />
        public Task<ChangeSet> SaveAsync()
        {
            return this.context.SaveChangesAsync();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.context.Dispose();
            }

            this.isDisposed = true;
        }
    }
}