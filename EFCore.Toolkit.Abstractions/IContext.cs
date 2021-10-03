using System;
using System.Threading.Tasks;

namespace EFCore.Toolkit.Abstractions
{    
     /// <summary>
     /// IContext is the database-independant abstraction of a data access context.
     /// </summary>
    public interface IContext : IDisposable
    {
        /// <summary>
        ///     Drops and recreates the underlying database.
        ///     USE WITH CARE!
        /// </summary>
        void ResetDatabase();

        /// <summary>
        ///     Drops the underlying database.
        /// </summary>
        void DropDatabase();

        /// <summary>
        ///     Sets the state of <paramref name="entity"/> to given modified.
        /// </summary>
        void SetStateModified<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        ///     Sets the state of <paramref name="entity"/> to given unchanged.
        /// </summary>
        void SetStateUnchanged<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        ///     Updates the properties of <paramref name="originalEntity" /> with values from <paramref name="updateEntity" />.
        /// </summary>
        TEntity SetValues<TEntity>(TEntity originalEntity, TEntity updateEntity) where TEntity : class;

        /// <summary>
        ///     Modifies the properties with <paramref name="propertyNames" /> of given entity <paramref name="entity" />.
        /// </summary>
        void ModifyProperties<TEntity>(TEntity entity, params string[] propertyNames) where TEntity : class;

        /// <summary>
        ///     Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>The number of objects written to the underlying database.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the context has been disposed.</exception>
        ChangeSet SaveChanges();

        /// <summary>
        ///     Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>The number of objects written to the underlying database.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the context has been disposed.</exception>
        Task<ChangeSet> SaveChangesAsync();

        ITransaction BeginTransaction();

        void UseTransaction(ITransaction transaction);
    }

    public interface ITransaction : IDisposable
    {
        /// <summary>
        ///     Commits all changes made to the database in the current transaction.
        /// </summary>
        void Commit();

        /// <summary>
        ///     Discards all changes made to the database in the current transaction.
        /// </summary>
        void Rollback();
    }
}