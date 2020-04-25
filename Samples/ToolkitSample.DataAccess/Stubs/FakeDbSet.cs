using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ToolkitSample.DataAccess.Stubs
{
    /// <summary>
    /// This is an in-memory, List backed implementation of
    /// Entity Framework's System.Data.Entity.IDbSet to use
    /// for testing.
    /// </summary>
    /// <typeparam name="T">The type of entity to store.</typeparam>
    public class FakeDbSet<T> : DbSet<T> where T : class
    {
        private readonly List<T> data;

        public FakeDbSet()
        {
            this.data = new List<T>();
        }

        public FakeDbSet(params T[] entities)
        {
            this.data = new List<T>(entities);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.data.GetEnumerator();
        }

        public Expression Expression
        {
            get { return Expression.Constant(this.data.AsQueryable()); }
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public IQueryProvider Provider
        {
            get { return this.data.AsQueryable().Provider; }
        }

        public T Find(params object[] keyValues)
        {
            throw new NotImplementedException("Wouldn't you rather use Linq .SingleOrDefault()?");
        }

        public T Add(T entity)
        {
            this.data.Add(entity);
            return entity;
        }

        public T Remove(T entity)
        {
            this.data.Remove(entity);
            return entity;
        }

        public T Attach(T entity)
        {
            this.data.Add(entity);
            return entity;
        }

        public T Create()
        {
            return Activator.CreateInstance<T>();
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
        {
            return Activator.CreateInstance<TDerivedEntity>();
        }

        public ObservableCollection<T> Local
        {
            get { return new ObservableCollection<T>(this.data); }
        }
    }
}
