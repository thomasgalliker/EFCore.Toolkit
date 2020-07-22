using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EFCore.Toolkit.Testing
{
    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            this.inner = inner;
        }

        public void Dispose()
        {
            inner.Dispose();
        }

        public T Current => inner.Current;

        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            return Task.FromResult(inner.MoveNext());
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(Task.FromResult(inner.MoveNext()));
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}