using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFCore.Toolkit.Auditing.Extensions;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Abstractions.Auditing;
using EFCore.Toolkit.Abstractions.Extensions;
using EFCore.Toolkit.Extensions;
using EFCore.Toolkit.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading.Tasks;

namespace EFCore.Toolkit.Auditing
{
    /// <summary>
    ///     AuditDbContextBase adds auditing capabilities to the DbContextBase.
    ///     Auditing is enabled by default but may be disabled (AuditEnabled=false) if needed.
    /// </summary>
    public abstract class AuditDbContextBase<TContext> : DbContextBase<TContext>, IAuditContext where TContext : DbContext
    {
        private const string AuditUpdatedColumnName = nameof(IAuditEntity.AuditDate);
        private const string AuditUserColumnName = nameof(IAuditEntity.AuditUser);
        private const string AuditTypeColumnName = nameof(IAuditEntity.AuditType);

        private static readonly IList<TContext> ConfigFileLock = new List<TContext>();
        private static readonly AuditDbContextConfiguration AuditDbContextConfiguration;

        private readonly Dictionary<Type, AuditTypeInfo> auditTypes = new Dictionary<Type, AuditTypeInfo>();

        static AuditDbContextBase()
        {
            lock (ConfigFileLock)
            {
                AuditDbContextConfiguration = AuditDbContextConfigurationManager.GetAuditDbContextConfigurationFromXml();
            }
        }

        /// <summary>
        ///     Empty constructor is used for 'update-database' command-line command.
        /// </summary>
        protected AuditDbContextBase()
        {
        }

        protected AuditDbContextBase(DbContextOptions dbContextOptions)
            : this(dbContextOptions, databaseInitializer: null, log: null)
        {
        }

        protected AuditDbContextBase(DbContextOptions dbContextOptions, Action<string> log)
            : this(dbContextOptions, databaseInitializer: null, log: log)
        {
        }

        protected AuditDbContextBase(DbContextOptions dbContextOptions, IDatabaseInitializer<TContext> databaseInitializer)
            : this(dbContextOptions, databaseInitializer, log: null)
        {
        }

        protected AuditDbContextBase(DbContextOptions dbContextOptions, IDatabaseInitializer<TContext> databaseInitializer, Action<string> log)
            : base(dbContextOptions, databaseInitializer, log)
        {
        }

        protected AuditDbContextBase(IDbConnection dbConnection, IDatabaseInitializer<TContext> databaseInitializer)
            : base(dbConnection, databaseInitializer)
        {
        }

        protected AuditDbContextBase(IDbConnection dbConnection, IDatabaseInitializer<TContext> databaseInitializer, Action<string> log)
            : base(dbConnection, databaseInitializer, log)
        {
        }

        /// <summary>
        ///     Initializes static members of the AuditDbContext class.
        /// </summary>
        protected void ConfigureAuditingFromAppConfig()
        {
            this.ConfigureAuditing(AuditDbContextConfiguration);
        }

        protected void ConfigureAuditing(AuditDbContextConfiguration configuration)
        {
            this.AuditEnabled = configuration.AuditEnabled;
            this.AuditDateTimeKind = configuration.AuditDateTimeKind;

            foreach (var auditTypeInfo in configuration.AuditTypeInfos)
            {
                this.RegisterAuditType(auditTypeInfo);
            }
        }

        /// <inheritdoc />
        public bool AuditEnabled { get; protected set; } = true;

        /// <inheritdoc />
        public DateTimeKind AuditDateTimeKind { get; protected set; }

        /// <summary>
        ///     Gets a value indicating whether this context is using proxies.
        /// </summary>
        ////public bool Proxies
        ////{
        ////    get
        ////    {
        ////        if (this.auditTypes.Count > 0)
        ////        {
        ////            var f = this.auditTypes.First();
        ////            var e = this.Set(f.Value.AuditableEntityType).Create();
        ////            return e.GetType().Namespace != f.Value.AuditableEntityType.Namespace;
        ////        }

        ////        return this.Database.Configuration.ProxyCreationEnabled;
        ////    }
        ////}

        /// <summary>
        ///     Registers and type for auditing.
        /// </summary>
        /// <param name="auditTypeInfo"></param>
        public void RegisterAuditType(AuditTypeInfo auditTypeInfo)
        {
            lock (this.auditTypes)
            {
                if (auditTypeInfo == null)
                {
                    throw new ArgumentNullException(nameof(auditTypeInfo));
                }

                // Extract the list of propeties to audit.
                var auditEntityType = auditTypeInfo.AuditEntityType;
                var auditEntityProperties = auditEntityType.GetRuntimeProperties();

                var auditableEntityType = auditTypeInfo.AuditableEntityType;
                var auditableEntityProperties = auditableEntityType.GetRuntimeProperties().ToDictionary(x => x.Name);

                if (this.auditTypes.ContainsKey(auditableEntityType))
                {
                    throw new ArgumentException($"Type {auditableEntityType.Name} is already registered for auditing.", nameof(auditableEntityType));
                }

                foreach (var property in auditEntityProperties)
                {
                    if (auditableEntityProperties.ContainsKey(property.Name))
                    {
                        if (property.PropertyType == auditableEntityProperties[property.Name].PropertyType)
                        {
                            auditTypeInfo.AuditProperties.Add(property.Name);
                        }
                    }
                }

                this.auditTypes.Add(auditableEntityType, auditTypeInfo);
            }
        }

        /// <summary>
        ///     Reloads the entity from the database overwriting any property values with values from the database.
        ///     The entity will be in the Unchanged state after calling this method.
        /// </summary>
        /// <param name="entity">The entity object to reload.</param>
        public void Reload(object entity)
        {
            this.Entry(entity).Reload();
        }

        /// <summary>
        ///     Saves all changes made in this context to the underlying database
        ///     using the current windows user for auditing.
        /// </summary>
        /// <returns>The number of objects written to the underlying database.</returns>
        public override ChangeSet SaveChanges()
        {
            // TODO Add Windows user here
            var username = "TODO";
            //var username = WindowsIdentity.GetCurrent().Name;
            return this.SaveChanges(username);
        }

        /// <summary>
        ///     Saves all changes made in this context to the underlying database
        ///     using the user parameter passed for auditing.
        /// </summary>
        /// <param name="username">User name for auditing.</param>
        /// <returns>The number of objects written to the underlying database.</returns>
        public ChangeSet SaveChanges(string username)
        {
            if (!this.AuditEnabled)
            {
                return base.SaveChanges();
            }

            var auditedEntities = this.AuditChanges(username).ToList();
            try
            {
                var changeSet = base.SaveChanges();
                this.OnAfterAuditChanges(auditedEntities);
                base.SaveChanges();

                return changeSet;
            }
            catch
            {
                this.RemoveAuditEntities(auditedEntities);
                throw;
            }
        }

        /// <inheritdoc />
        public override Task<ChangeSet> SaveChangesAsync()
        {
            // TODO Add Windows user here
            var username = "TODO";
            //var username = WindowsIdentity.GetCurrent().Name;
            return this.SaveChangesAsync(username);
        }

        /// <summary>
        ///     Saves all changes made in this context to the underlying database
        ///     using the user parameter passed for auditing.
        /// </summary>
        /// <param name="username">User name for auditing.</param>
        /// <returns>The number of objects written to the underlying database.</returns>
        public async Task<ChangeSet> SaveChangesAsync(string username)
        {
            if (!this.AuditEnabled)
            {
                return await base.SaveChangesAsync();
            }

            var auditedEntities = this.AuditChanges(username).ToList();
            try
            {
                var changeSet = await base.SaveChangesAsync();
                this.OnAfterAuditChanges(auditedEntities);
                await base.SaveChangesAsync();

                return changeSet;
            }
            catch
            {
                this.RemoveAuditEntities(auditedEntities);
                throw;
            }
        }

        /// <summary>
        /// Update failed so remove the audit entities.
        /// </summary>
        private void RemoveAuditEntities(IReadOnlyCollection<AuditedEntity> auditedEntities)
        {
            var firstOrDefault = auditedEntities.FirstOrDefault();
            if (firstOrDefault != null)
            {
                dynamic dbSet = this.Set(firstOrDefault.AuditEntity.GetType());

                foreach (dynamic auditEntity in auditedEntities.Select(a => a.AuditEntity))
                {
                    dbSet.Remove(auditEntity);
                }
            }
        }

        // Track all audit entities created in this transaction, will be removed from context on exception.
        private IEnumerable<AuditedEntity> AuditChanges(string username)
        {
            // Use the same datetime for all updates in this transaction, retrieved from server when first used.
            DateTime? dateTimeNow = null;

            // Process any auditable objects.
            var trackedEntries = this.ChangeTracker.Entries().ToList();
            foreach (var entry in trackedEntries)
            {
                if (entry.Entity is IAuditEntity || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                {
                    continue;
                }

                if (dateTimeNow.HasValue == false)
                {
                    dateTimeNow = DateTime.UtcNow.ToKind(this.AuditDateTimeKind);
                }

                var creatableEntity = entry.Entity as ICreatedDate;
                if (entry.State == EntityState.Added && creatableEntity != null)
                {
                    creatableEntity.CreatedDate = dateTimeNow.Value;
                }

                var updateableEntity = entry.Entity as IUpdatedDate;
                if (entry.State == EntityState.Modified)
                {
                    if (creatableEntity != null)
                    {
                        entry.Property(nameof(ICreatedDate.CreatedDate)).IsModified = false;
                    }

                    if (updateableEntity != null)
                    {
                        updateableEntity.UpdatedDate = dateTimeNow.Value;
                    }
                }

                var entityType = entry.GetEntityType();
                var auditTypeInfo = this.GetAuditTypeInfo(entityType);
                if (auditTypeInfo != null)
                {
                    var auditEntity = this.AuditEntity(entry, auditTypeInfo, dateTimeNow.Value, username);
                    yield return auditEntity;
                }
            }
        }

        private AuditTypeInfo GetAuditTypeInfo(Type entityType)
        {
            lock (this.auditTypes)
            {
                if (this.auditTypes.ContainsKey(entityType) && this.auditTypes[entityType].AuditEntityType != null)
                {
                    return this.auditTypes[entityType];
                }
            }

            return null;
        }

        private AuditedEntity AuditEntity(EntityEntry entityEntry, AuditTypeInfo auditTypeInfo, DateTime auditDateTime, string user)
        {
            // Create audit entity.
            dynamic dbSet = this.Set(auditTypeInfo.AuditEntityType);
            dynamic auditEntity = (IAuditEntity)Activator.CreateInstance(auditTypeInfo.AuditEntityType);
            dbSet.Add(auditEntity);

            // Store all temporary values (e.g. not-yet generated primary keys) for later update
            var temporaryProperties = entityEntry.Properties.Where(p => p.IsTemporary).ToList();

            // Copy the properties.
            var auditEntityEntry = this.Entry(auditEntity);
            if (entityEntry.State == EntityState.Added)
            {
                foreach (var propertyName in auditTypeInfo.AuditProperties)
                {
                    auditEntityEntry.Property(propertyName).CurrentValue = entityEntry.Property(propertyName).CurrentValue;
                }
            }
            else
            {
                foreach (var propertyName in auditTypeInfo.AuditProperties)
                {
                    auditEntityEntry.Property(propertyName).CurrentValue = entityEntry.Property(propertyName).OriginalValue;
                }
            }

            // Set the audit columns.
            auditEntityEntry.Property(AuditUpdatedColumnName).CurrentValue = auditDateTime;
            auditEntityEntry.Property(AuditUserColumnName).CurrentValue = user;
            auditEntityEntry.Property(AuditTypeColumnName).CurrentValue = entityEntry.State.ToAuditEntityState();

            return new AuditedEntity(auditEntity, temporaryProperties);
        }

        private class AuditedEntity
        {
            public IAuditEntity AuditEntity { get; }
            public IEnumerable<PropertyEntry> TemporaryProperties { get; }

            public AuditedEntity(IAuditEntity auditEntity, IEnumerable<PropertyEntry> temporaryProperties)
            {
                this.AuditEntity = auditEntity;
                this.TemporaryProperties = temporaryProperties;
            }
        }

        private void OnAfterAuditChanges(IReadOnlyCollection<AuditedEntity> auditedEntities)
        {
            if (auditedEntities == null || auditedEntities.Count == 0)
            {
                return;
            }

            foreach (var auditedEntity in auditedEntities)
            {
                // Get the final value of the temporary properties
                foreach (var prop in auditedEntity.TemporaryProperties)
                {
                    auditedEntity.AuditEntity.SetPropertyValue(prop.Metadata.Name, prop.CurrentValue);
                }
            }
        }
    }
}