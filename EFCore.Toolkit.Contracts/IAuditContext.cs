﻿using System.Threading.Tasks;

namespace EFCore.Toolkit.Abstractions
{
    public interface IAuditContext : IContext
    {
        /// <summary>
        ///     Specifies if the auditing feature is enabled.
        /// </summary>
        bool AuditEnabled { get; }

        /// <summary>
        ///     Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>The number of objects written to the underlying database.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the context has been disposed.</exception>
        ChangeSet SaveChanges(string username);

        /// <summary>
        ///     Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>The number of objects written to the underlying database.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the context has been disposed.</exception>
        Task<ChangeSet> SaveChangesAsync(string username);
    }
}