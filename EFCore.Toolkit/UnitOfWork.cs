#if !NETSTANDARD1_3
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Transactions;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Exceptions;
using EFCore.Toolkit.Extensions;
#if !NET40
using System.Threading.Tasks;
#endif

namespace EFCore.Toolkit
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly Dictionary<Type, IContext> contexts;

        private bool disposed;

        public UnitOfWork()
        {
            this.contexts = new Dictionary<Type, IContext>();
        }

        public void RegisterContext<TContext>(TContext context) where TContext : IContext
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var contextType = context.GetType();

            if (!this.contexts.ContainsKey(contextType))
            {
                this.contexts.Add(contextType, context);
            }
            else
            {
                throw new InvalidOperationException($"Context '{contextType.GetFormattedName()}' is already registered.");
            }
        }

        /// <inheritdoc />
        public ICollection<ChangeSet> Commit()
        {
            var changeSets = new Collection<ChangeSet>();
            Type lastContextType = null;
            try
            {
                var firstContext = this.contexts.FirstOrDefault();
                if (firstContext.Value != null)
                {
                    var transaction = firstContext.Value.BeginTransaction();
                    foreach (var context in this.contexts)
                    {
                        if (context.Value != firstContext.Value)
                        {
                            context.Value.UseTransaction(transaction);
                        }

                        lastContextType = context.Key;
                        var changes = context.Value.SaveChanges();
                        changeSets.Add(changes);
                    }

                    // Commit transaction if all commands succeed, transaction will auto-rollback
                    // when disposed if either commands fails
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new UnitOfWorkException($"UnitOfWork in context '{lastContextType?.Name}' failed to commit.", ex);
            }

            return changeSets;
        }

#if !NET40
        /// <inheritdoc />
        public async Task<ICollection<ChangeSet>> CommitAsync()
        {
            var changeSets = new Collection<ChangeSet>();
            Type lastContextType = null;
            try
            {
                var firstContext = this.contexts.FirstOrDefault();
                if (firstContext.Value != null)
                {
                    var transaction = firstContext.Value.BeginTransaction();
                    foreach (var context in this.contexts)
                    {
                        if (context.Value != firstContext.Value)
                        {
                            context.Value.UseTransaction(transaction);
                        }

                        lastContextType = context.Key;
                        var changes = await context.Value.SaveChangesAsync();
                        changeSets.Add(changes);
                    }

                    // Commit transaction if all commands succeed, transaction will auto-rollback
                    // when disposed if either commands fails
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new UnitOfWorkException($"UnitOfWork in context '{lastContextType?.Name}' failed to commit.", ex);
            }

            return changeSets;
        }
#endif

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    foreach (var c in this.contexts.Values)
                    {
                        c.Dispose();
                    }
                    this.contexts.Clear();
                }

                this.disposed = true;
            }
        }

        ~UnitOfWork()
        {
            this.Dispose(false);
        }
    }
}
#endif