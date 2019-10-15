using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using EFCore.Toolkit.Concurrency;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Exceptions;
using EFCore.Toolkit.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using IDbConnection = EFCore.Toolkit.Abstractions.IDbConnection;
#if !NET40
using System.Threading.Tasks;

#endif

namespace EFCore.Toolkit
{
    public abstract class DbContextBase<TContext> : DbContext, IDbContext
        where TContext : DbContext
    {
        private static readonly IList<TContext> InitializerLock = new List<TContext>();
        private readonly IDatabaseInitializer<TContext> databaseInitializer;
        private readonly IDbConnection dbConnection;

        /// <summary>
        ///     Empty constructor is used for 'update-database' command-line command.
        /// </summary>
        protected DbContextBase()
        {
            //TryInitializeDatabase(this, null);
        }

        protected DbContextBase(DbContextOptions dbContextOptions)
            : this(dbContextOptions, databaseInitializer: null, log: null)
        {
        }

        protected DbContextBase(DbContextOptions dbContextOptions, IDatabaseInitializer<TContext> databaseInitializer)
            : this(dbContextOptions, databaseInitializer, log: null)
        {
        }
        
        protected DbContextBase(DbContextOptions dbContextOptions, Action<string> log)
            : this(dbContextOptions, databaseInitializer: null, log: log)
        {
        }

        protected DbContextBase(DbContextOptions dbContextOptions, IDatabaseInitializer<TContext> databaseInitializer, Action<string> log)
            : base(dbContextOptions)
        {
            this.EnsureLog(log);

            this.log($"Initializing DbContext '{this.Name}' with NameOrConnectionString = \"{this.GetConnectionString()}\" and IDatabaseInitializer =\"{databaseInitializer?.GetType().GetFormattedName()}\"");

            this.databaseInitializer = databaseInitializer;
            this.TryInitializeDatabase();
        }

        private string GetConnectionString()
        {
            return this.dbConnection?.ConnectionString ?? null;
        }

        protected DbContextBase(IDbConnection dbConnection, IDatabaseInitializer<TContext> databaseInitializer)
            : this(dbConnection, databaseInitializer, log: null)
        {
        }

        protected DbContextBase(IDbConnection dbConnection, IDatabaseInitializer<TContext> databaseInitializer, Action<string> log)
            : this()
        {
            this.EnsureLog(log);
            this.dbConnection = dbConnection;

            this.log($"Initializing DbContext '{this.Name}' with ConnectionString = \"{this.GetConnectionString()}\" and IDatabaseInitializer=\"{databaseInitializer?.GetType().GetFormattedName()}\"");

            this.databaseInitializer = databaseInitializer;
            this.TryInitializeDatabase();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            this.log($"{this.Name}.OnConfiguring");

            if (this.GetConnectionString() is string connectionString && connectionString != null)
            {
                optionsBuilder.UseSqlServer(connectionString);
            }

            //if (!optionsBuilder.Options.Extensions.Any(extension => extension.GetType().Name == "InMemoryOptionsExtension"))
            //{
            //    optionsBuilder.UseSqlServer(connectionString);
            //}

            //optionsBuilder.UseLoggerFactory(new Consol)
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            this.log($"{this.Name}.OnModelCreating");
     
            ////modelBuilder.Remove<PluralizingTableNameConvention>();

#if !NETSTANDARD1_3 && !NETFX
            modelBuilder.Query<TableRowCounts>();
#endif
        }


        private void EnsureLog(Action<string> log = null)
        {
            if (log == null)
            {
                log = s => Debug.WriteLine(s);
            }

            this.log = message => log(message);
        }

        private Action<string> log { get; set; }

        /// <inheritdoc />
        public string Name { get; } = typeof(TContext).GetFormattedName();

        private void TryInitializeDatabase(bool force = false)
        {
            if (this.databaseInitializer == null)
            {
                return;
            }

            try
            {
                lock (InitializerLock)
                {
                    this.log($"TryInitializeDatabase: this.databaseInitializer={this.databaseInitializer.GetType().GetFormattedName()}, force={force}");
                    this.databaseInitializer.Initialize(this, force);
                }
            }
            catch (Exception ex)
            {
                this.log($"TryInitializeDatabase failed with exception: {ex}");
            }
        }

        /// <inheritdoc />
        ////public IDbSet<TEntity> DbSet<TEntity>() where TEntity : class
        ////{
        ////    return base.Set<TEntity>();
        ////}

        /// <inheritdoc />
        public void ResetDatabase()
        {
            this.log($"ResetDatabase of DbContext '{this.Name}' with ConnectionString = \"{this.GetConnectionString()}\"");

            this.InternalResetDatabase();
        }

        private void InternalResetDatabase()
        {
            this.InternalDropDatabase();

            this.TryInitializeDatabase(force: true);
        }

        /// <inheritdoc />
        public void DropDatabase()
        {
            this.log($"DropDatabase of DbContext '{this.Name}' with ConnectionString = \"{this.GetConnectionString()}\"");

            this.InternalDropDatabase();
        }

        private void InternalDropDatabase()
        {
            this.Database.KillConnectionsToTheDatabase();
            this.Database.EnsureDeleted();
        }

        /// <inheritdoc />
        public TEntity Edit<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentException(nameof(entity));
            }

            this.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        /// <inheritdoc />
        public TEntity Edit<TEntity>(TEntity originalEntity, TEntity updateEntity) where TEntity : class
        {
            if (originalEntity == null)
            {
                throw new ArgumentException(nameof(originalEntity));
            }

            if (updateEntity == null)
            {
                throw new ArgumentException(nameof(updateEntity));
            }

            var attachedEntry = this.Entry(originalEntity);
            if (attachedEntry.State == EntityState.Detached)
            {
                this.Set<TEntity>().Attach(originalEntity);
            }

            attachedEntry.CurrentValues.SetValues(updateEntity);
            return originalEntity;
        }

        /// <inheritdoc />
        public TEntity Delete<TEntity>(TEntity entity) where TEntity : class
        {
            this.Entry(entity).State = EntityState.Deleted;
            return entity;
        }

        /// <inheritdoc />
        public void UndoChanges<TEntity>(TEntity entity) where TEntity : class
        {
            this.Entry(entity).State = EntityState.Unchanged;
        }

        /// <inheritdoc />
        public void ModifyProperties<TEntity>(TEntity entity, params string[] propertyNames) where TEntity : class
        {
            var entry = this.Entry(entity);

            foreach (var propertyName in propertyNames)
            {
                entry.Property(propertyName).IsModified = true;
            }

            //if (property.Metadata.IsShadowProperty)
            //{
            //    // BUG: Workaround for resetting IsModified of Discriminator property
            //    property.IsModified = false;
            //}
        }

        /// <inheritdoc />
        public void LoadReferenced<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> navigationProperty) where TEntity : class where TProperty : class
        {
            this.Entry(entity).Reference(navigationProperty).Load();
        }

        /// <inheritdoc />
        public new virtual ChangeSet SaveChanges()
        {
            var changeSet = this.GetChangeSet();
            try
            {
                base.SaveChanges();
            }
            ////catch (DbEntityValidationException validationException)
            ////{
            ////    string errorMessage = validationException.GetFormattedErrorMessage();
            ////    throw new DbEntityValidationException(errorMessage, validationException);
            ////}
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                this.HandleDbUpdateConcurrencyException(dbUpdateConcurrencyException);

                //TODO: Handle number of max retries
                return ((IContext)this).SaveChanges();
            }
            catch (DbUpdateException dbUpdateException)
            {
                string errorMessage = dbUpdateException.GetFormattedErrorMessage();
                throw new DbUpdateException(errorMessage, dbUpdateException);
            }

            return changeSet;
        }

        /// <inheritdoc />
        public IConcurrencyResolveStrategy ConcurrencyResolveStrategy { get; set; } = new RethrowConcurrencyResolveStrategy();

#if !NET40
        /// <inheritdoc />
        public virtual async Task<ChangeSet> SaveChangesAsync()
        {
            var changeSet = this.GetChangeSet();
            try
            {
                await base.SaveChangesAsync();
            }
            ////catch (DbEntityValidationException validationException)
            ////{
            ////    string errorMessage = validationException.GetFormattedErrorMessage();
            ////    throw new DbEntityValidationException(errorMessage, validationException);
            ////}
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                this.HandleDbUpdateConcurrencyException(dbUpdateConcurrencyException);

                //TODO: Handle number of max retries
                return await ((IContext)this).SaveChangesAsync();
            }
            catch (DbUpdateException dbUpdateException)
            {
                string errorMessage = dbUpdateException.GetFormattedErrorMessage();
                throw new DbUpdateException(errorMessage, dbUpdateException);
            }

            return changeSet;
        }

        public ITransaction BeginTransaction()
        {
            var dbContextTransaction = this.Database.BeginTransaction();
            return new InternalDbContextTransaction(dbContextTransaction);
        }

        public void UseTransaction(ITransaction transaction)
        {
            var internalDbContextTransaction = (InternalDbContextTransaction)transaction;
            var dbTransaction = internalDbContextTransaction.GetDbTransaction();
            this.Database.UseTransaction(dbTransaction);
        }
#endif

        private void HandleDbUpdateConcurrencyException(DbUpdateConcurrencyException dbUpdateConcurrencyException)
        {
            // Get the current entity values and the values in the database 
            // as instances of the entity type 
            var entry = dbUpdateConcurrencyException.Entries.Single();
            var databaseValues = entry.GetDatabaseValues();
            if (databaseValues == null)
            {
                throw new UpdateConcurrencyException("Failed to update an entity which which has previsouly been deleted.", dbUpdateConcurrencyException);
            }

            var databaseValuesAsObject = databaseValues.ToObject();
            var conflictingEntity = entry.Entity;

            // Have the user choose what the resolved values should be 
            var resolvedValuesAsObject = this.ConcurrencyResolveStrategy.ResolveConcurrencyException(conflictingEntity, databaseValuesAsObject);
            if (resolvedValuesAsObject == null || this.ConcurrencyResolveStrategy is RethrowConcurrencyResolveStrategy)
            {
                if (this.ConcurrencyResolveStrategy is RethrowConcurrencyResolveStrategy)
                {
                    throw dbUpdateConcurrencyException;
                }
            }

            // Update the original values with the database values and 
            // the current values with whatever the user choose. 
            entry.OriginalValues.SetValues(databaseValues);
            entry.CurrentValues.SetValues(resolvedValuesAsObject);
        }

        /// <summary>
        ///     Determines the changes that are transferred to the persistence layer.
        /// </summary>
        /// <returns>ChangeSet.</returns>
        private ChangeSet GetChangeSet()
        {
            var updatedEntries = this.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified && e.Entity != null);

            var updateChanges = new List<IChange>();

            foreach (var dbEntityEntry in updatedEntries)
            {
                IList<PropertyChangeInfo> changes = new List<PropertyChangeInfo>();
                foreach (var propertyName in dbEntityEntry.CurrentValues.Properties.Select(p => p.Name))
                {
                    var property = dbEntityEntry.Property(propertyName);
                    if (property.Metadata.IsShadowProperty)
                    {
                        // BUG: Workaround for resetting IsModified of Discriminator property
                        property.IsModified = false;
                    }

                    if (property.IsModified)
                    {
                        changes.Add(new PropertyChangeInfo(propertyName, property.CurrentValue));
                    }
                }
                updateChanges.Add(Change.CreateUpdateChange(dbEntityEntry.Entity, changes));
            }

            var addChanges = this.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Entity != null)
                .Select(e => Change.CreateAddedChange(e.Entity));

            var deleteChanges = this.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Deleted && e.Entity != null)
                .Select(n => Change.CreateDeleteChange(n.Entity));

            var allChanges = new List<IChange>(addChanges);
            allChanges.AddRange(deleteChanges);
            allChanges.AddRange(updateChanges);

            return new ChangeSet(typeof(TContext), allChanges);
        }

        /// <inheritdoc />
        public bool IsDisposed { get; private set; }

        /// <inheritdoc />
        public override void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                base.Dispose();
            }

            this.IsDisposed = true;
        }
    }

    public class InternalDbContextTransaction : ITransaction
    {
        private readonly IDbContextTransaction dbContextTransaction;

        public InternalDbContextTransaction(IDbContextTransaction dbContextTransaction)
        {
            this.dbContextTransaction = dbContextTransaction;
        }

        public void Dispose()
        {
            this.dbContextTransaction?.Dispose();
        }

        public void Commit()
        {
            this.dbContextTransaction.Commit();
        }

        public void Rollback()
        {
            this.dbContextTransaction.Rollback();
        }

        public DbTransaction GetDbTransaction()
        {
            return this.dbContextTransaction.GetDbTransaction();
        }
    }
}