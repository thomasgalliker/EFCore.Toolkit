using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if !NET40

#endif

namespace EFCore.Toolkit.Abstractions
{
    public interface IUnitOfWork : IDisposable
    {
        void RegisterContext<TContext>(TContext context) where TContext : IContext;

        /// <summary>
        /// Saves pending changes to all registered contexts.
        /// </summary>
        /// <returns>The total number of objects committed.</returns>
        ICollection<ChangeSet> Commit();

#if !NET40
        /// <summary>
        /// Saves pending changes to all registered contexts.
        /// </summary>
        /// <returns>The total number of objects committed.</returns>
        Task<ICollection<ChangeSet>> CommitAsync();
#endif
    }
}
