using System;
using System.Collections.Generic;

#if !NET40
using System.Threading.Tasks;
#endif

namespace EFCore.Toolkit.Contracts
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
