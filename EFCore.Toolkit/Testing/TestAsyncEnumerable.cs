using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EFCore.Toolkit.Testing
{
    /// <summary>
    /// TestAsyncEnumerable&lt;T&gt; implements <seealso cref="IAsyncEnumerable&lt;T&gt;"/>
    /// which is used to mock collections behind async queryables -> ToListAsync
    /// 
    /// Source: https://stackoverflow.com/questions/40476233/how-to-mock-an-async-repository-with-entity-framework-core
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        {
        }

        public IAsyncEnumerator<T> GetEnumerator()
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }
}