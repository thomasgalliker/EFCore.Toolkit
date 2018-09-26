using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using EntityFramework.Toolkit.EFCore.Concurrency;
using EntityFramework.Toolkit.EFCore.Contracts;
using EntityFramework.Toolkit.EFCore.Exceptions;
using EntityFramework.Toolkit.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;
#if !NET40
using System.Threading.Tasks;

#endif

namespace EntityFramework.Toolkit.EFCore
{
    public abstract class DbContextBase<TContext> : DbContext, IDbContext
        where TContext : DbContext
    {
        private static readonly IList<TContext> InitializerLock = new List<TContext>();

        private readonly string contextName = typeof(TContext).GetFormattedName();

        /// <summary>
        ///     Empty constructor is used for 'update-database' command-line command.
        /// </summary>
        protected DbContextBase()
        {
            //TryInitializeDatabase(this, null);
        }

        protected DbContextBase(string nameOrConnectionString, IDatabaseInitializer<TContext> databaseInitializer)
            : this(nameOrConnectionString, databaseInitializer, log: null)
        {
        }

        protected DbContextBase(string nameOrConnectionString, IDatabaseInitializer<TContext> databaseInitializer, Action<string> log)
            : base(nameOrConnectionString)
        {
            this.EnsureLog(log);
            this.Name = this.contextName;

            this.Database.Log($"Initializing DbContext '{this.contextName}' with NameOrConnectionString = \"{nameOrConnectionString}\" and IDatabaseInitializer =\"{databaseInitializer?.GetType().GetFormattedName()}\"");

            TryInitializeDatabase(this, databaseInitializer, log);
        }

        protected DbContextBase(IDbConnection dbConnection, IDatabaseInitializer<TContext> databaseInitializer)
            : this(dbConnection, databaseInitializer, log: null)
        {
        }

        protected DbContextBase(IDbConnection dbConnection, IDatabaseInitializer<TContext> databaseInitializer, Action<string> log)
            : this()
        {
            this.EnsureLog(log);

            this.Database.Connection.ConnectionString = dbConnection.ConnectionString;
            this.Configuration.LazyLoadingEnabled = dbConnection.LazyLoadingEnabled;
            this.Configuration.ProxyCreationEnabled = dbConnection.ProxyCreationEnabled;
            this.Name = dbConnection.Name ?? this.GetType().GetFormattedName();

            this.Database.Log(
                $"Initializing DbContext '{this.contextName}' with ConnectionString = \"{dbConnection.ConnectionString}\" and IDatabaseInitializer=\"{databaseInitializer?.GetType().GetFormattedName()}\"");

            TryInitializeDatabase(this, databaseInitializer, log);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(new Consol)
        }

        private void EnsureLog(Action<string> log = null)
        {
            if (log == null)
            {
                log = s => Debug.WriteLine(s);
            }

            this.Database.Log = message => log(message);
        }

        /// <inheritdoc />
        public string Name { get; private set; }

        private static void TryInitializeDatabase(DbContext dbContext, IDatabaseInitializer<TContext> databaseInitializer, Action<string> log)
        {
            try
            {
                lock (InitializerLock)
                {
                    Database.SetInitializer(databaseInitializer);
                    dbContext.Database.Initialize(force: false);
                }
            }
            catch (Exception ex)
            {
                log(ex.ToString());
            }
        }

        /// <inheritdoc />
        public IDbSet<TEntity> DbSet<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }

        /// <inheritdoc />
        public void ResetDatabase()
        {
            this.Database.Log($"ResetDatabase of DbContext '{this.contextName}' with ConnectionString = \"{this.Database.Connection.ConnectionString}\"");

            this.InternalResetDatabase();
        }

        private void InternalResetDatabase()
        {
            this.InternalDropDatabase();

            this.Database.Initialize(force: true);
        }

        /// <inheritdoc />
        public void DropDatabase()
        {
            this.Database.Log($"DropDatabase of DbContext '{this.contextName}' with ConnectionString = \"{this.Database.Connection.ConnectionString}\"");

            this.InternalDropDatabase();
        }

        private void InternalDropDatabase()
        {
            this.Database.KillConnectionsToTheDatabase();
            this.Database.Delete();
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
            catch (DbEntityValidationException validationException)
            {
                string errorMessage = validationException.GetFormattedErrorMessage();
                throw new DbEntityValidationException(errorMessage, validationException);
            }
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
        public new virtual async Task<ChangeSet> SaveChangesAsync()
        {
            var changeSet = this.GetChangeSet();
            try
            {
                await base.SaveChangesAsync();
            }
            catch (DbEntityValidationException validationException)
            {
                string errorMessage = validationException.GetFormattedErrorMessage();
                throw new DbEntityValidationException(errorMessage, validationException);
            }
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

        /// <inheritdoc />
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        /// <summary>
        ///     Determins the changes that are transfered to the persistence layer.
        /// </summary>
        /// <returns>ChangeSet.</returns>
        private ChangeSet GetChangeSet()
        {
            var updatedEntries = this.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified && e.Entity != null);
            var updateChanges = new List<IChange>();

            foreach (var dbEntityEntry in updatedEntries)
            {
                IList<PropertyChangeInfo> changes = new List<PropertyChangeInfo>();
                foreach (var propertyName in dbEntityEntry.CurrentValues.PropertyNames)
                {
                    var property = dbEntityEntry.Property(propertyName);
                    if (property.IsModified)
                    {
                        changes.Add(new PropertyChangeInfo(propertyName, property.CurrentValue));
                    }
                }
                updateChanges.Add(Change.CreateUpdateChange(dbEntityEntry.Entity, changes));
            }

            var addChanges = this.ChangeTracker.Entries().Where(e => e.State == EntityState.Added && e.Entity != null).Select(e => Change.CreateAddedChange(e.Entity));
            var deleteChanges = this.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted && e.Entity != null).Select(n => Change.CreateDeleteChange(n.Entity));

            var allChanges = new List<IChange>(addChanges);
            allChanges.AddRange(deleteChanges);
            allChanges.AddRange(updateChanges);

            return new ChangeSet(typeof(TContext), allChanges);
        }

        /// <inheritdoc />
        public bool IsDisposed { get; private set; }

        /// <inheritdoc />
        /// :
        protected override void Dispose(bool disposing)
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose(disposing);

            this.IsDisposed = true;
        }
    }
}