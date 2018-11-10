using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EFCore.Toolkit.Contracts;
using EFCore.Toolkit.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCore.Toolkit.Extensions
{
    public static class DbContextExtensions
    {
        public static bool AllMigrationsApplied(this DbContext context)
        {
            var applied = context.GetService<IHistoryRepository>()
                .GetAppliedMigrations()
                .Select(m => m.MigrationId);

            var total = context.GetService<IMigrationsAssembly>()
                .Migrations
                .Select(m => m.Key);

            return !total.Except(applied).Any();
        }

        public static void Seed(this DbContext context, IEnumerable<IDataSeed> dataSeeds)
        {
            foreach (var dataSeed in dataSeeds)
            {
                var predicate = dataSeed.GetAddOrUpdateExpression();

                ReflectionHelper.InvokeGenericMethod(
                    null,
                    () => DbContextExtensions.AddOrUpdate<object>(null, null, null),
                    dataSeed.EntityType,
                    new object[] { context, predicate, dataSeed.GetAllObjects() });
            }
        }

        public static void AddOrUpdate<TEntity>(this DbContext context, Expression<Func<object, object>> propertyExpression, params object[] entities) where TEntity : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            var set = context.Set<TEntity>();
            var parameter = Expression.Parameter(typeof(TEntity));
            var propertyName = propertyExpression.GetPropertyInfo().Name;
            var property = Expression.Property(parameter, propertyName);
            foreach (var entity in entities)
            {
                var propertyValue = entity.GetPropertyValue(propertyName);
                var equalExpression = Expression.Equal(property, Expression.Constant(propertyValue));
                var lambdaExpression = Expression.Lambda<Func<TEntity, bool>>(equalExpression, parameter);
                var existingEntity = set.SingleOrDefault(lambdaExpression);
                if (existingEntity != null)
                {
                    context.Entry(existingEntity).CurrentValues.SetValues(entity);
                }
                else
                {
                    context.Entry(entity).State = EntityState.Added;
                }
            }
        }

        /// <summary>
        ///     Adds an entity (if newly created) or update (if has non-default Id).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The db context.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <remarks>
        ///     Will not work for HasDatabaseGeneratedOption(DatabaseGeneratedOption.None).
        ///     Will not work for composite keys.
        /// </remarks>
        public static T AddOrUpdate<T>(this DbContext context, T entity) where T : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (IsTransient(context, entity))
            {
                context.Set<T>().Add(entity);
            }
            else
            {
                context.Set<T>().Attach(entity);
                context.Entry(entity).State = EntityState.Modified;
            }
            return entity;
        }

        /// <summary>
        ///     Determines whether the specified entity is newly created (Id not specified).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///     <c>true</c> if the specified entity is transient; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        ///     Will not work for HasDatabaseGeneratedOption(DatabaseGeneratedOption.None).
        ///     Will not work for composite keys.
        /// </remarks>
        public static bool IsTransient<T>(this DbContext context, T entity) where T : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var primaryKey = GetPrimaryKeyFor<T>(context);
            var propertyType = primaryKey.PropertyInfo.PropertyType;
            //what's the default value for the type?
            var transientValue = propertyType.GetTypeInfo().IsValueType ? Activator.CreateInstance(propertyType) : null;
            //is the pk the same as the default value (int == 0, string == null ...)
            return Equals(primaryKey.PropertyInfo.GetValue(entity, null), transientValue);
        }

        /// <summary>
        ///     Loads a stub entity (or actual entity if already loaded).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks>
        ///     Will not work for composite keys.
        /// </remarks>
        public static T Load<T>(this DbContext context, object id) where T : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var primaryKey = GetPrimaryKeyFor<T>(context);
            //check to see if it's already loaded (slow if large numbers loaded)
            var entity = context.Set<T>().Local.SingleOrDefault(x => id.Equals(primaryKey.PropertyInfo.GetValue(x, null)));
            if (entity == null)
            {
                //it's not loaded, just create a stub with only primary key set
                entity = CreateEntity<T>(id, primaryKey.PropertyInfo);

                context.Set<T>().Attach(entity);
            }
            return entity;
        }

        /// <summary>
        ///     Determines whether the specified entity is loaded from the database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="id">The id.</param>
        /// <returns>
        ///     <c>true</c> if the specified entity is loaded; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        ///     Will not work for composite keys.
        /// </remarks>
        public static bool IsLoaded<T>(this DbContext context, object id) where T : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var primaryKey = GetPrimaryKeyFor<T>(context);
            //check to see if it's already loaded (slow if large numbers loaded)
            var entity = context.Set<T>().Local.SingleOrDefault(x => id.Equals(primaryKey.PropertyInfo.GetValue(x, null)));
            return entity != null;
        }

        /// <summary>
        ///     Marks the reference navigation properties unchanged.
        ///     Use when adding a new entity whose references are known to be unchanged.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="entity">The entity.</param>

        /// <summary>
        ///     Merges a DTO into a new or existing entity attached/added to context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="dataTransferObject">
        ///     The data transfer object. It must have a primary key property of the same name and
        ///     type as the actual entity.
        /// </param>
        /// <returns></returns>
        /// <remarks>
        ///     Will not work for composite keys.
        /// </remarks>
        public static T Merge<T>(this DbContext context, T dataTransferObject) where T : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (dataTransferObject == null)
            {
                throw new ArgumentNullException(nameof(dataTransferObject));
            }

            //find the id property of the dto
            var primaryKey = context.GetPrimaryKeyForEntity(dataTransferObject);

            //has the id been set (existing item) or not (transient)?
            var propertyType = primaryKey.PropertyInfo.PropertyType;
            var transientValue = propertyType.GetTypeInfo().IsValueType ? Activator.CreateInstance(propertyType) : null;
            var isTransient = Equals(primaryKey.Value, transientValue);

            T entity;
            if (isTransient)
            {
                //it's transient, just create a dummy
                entity = CreateEntity<T>(primaryKey.Value, primaryKey.PropertyInfo);
                //if DatabaseGeneratedOption(DatabaseGeneratedOption.None) and no id, this errors
                context.Set<T>().Attach(entity);
            }
            else
            {
                //try to load from identity map or database
                entity = context.Set<T>().Find(primaryKey.Value);
                if (entity == null)
                {
                    //could not find entity, assume assigned primary key
                    entity = CreateEntity<T>(primaryKey.Value, primaryKey.PropertyInfo);
                    context.Set<T>().Add(entity);
                }
            }
            //copy the values from DTO onto the entry
            context.Entry(entity).CurrentValues.SetValues(dataTransferObject);
            return entity;
        }

        /// <summary>
        /// Returns the primary key <see cref="PropertyInfo"/> for a given type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="context">The context in which the given entity type lives.</param>
        /// <returns></returns>
        public static PrimaryKey GetPrimaryKeyFor<TEntity>(this DbContext context) where TEntity : class
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //find the primary key
            var elementType = context.GetElementType(typeof(TEntity));
            var primaryKey = elementType.FindPrimaryKey().Properties.First();

            //look it up on the entity
            var propertyInfo = typeof(TEntity).GetRuntimeProperty(primaryKey.Name);
            return propertyInfo == null ? null : new PrimaryKey(propertyInfo, null);
        }

        /// <summary>
        /// Returns the primary key for the given entity.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity.</typeparam>
        /// <param name="context">The target context.</param>
        /// <param name="entity">The given entity.<</param>
        /// <returns></returns>
        public static PrimaryKey GetPrimaryKeyForEntity<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            var primaryKey = context.GetPrimaryKeyFor<TEntity>();
            if (primaryKey == null)
            {
                throw new InvalidOperationException("Cannot find an id on the dataTransferObject");
            }

            var value = primaryKey.PropertyInfo.GetValue(entity, null);
            return new PrimaryKey(primaryKey.PropertyInfo, value);
        }

        private static T CreateEntity<T>(object id, PropertyInfo property) where T : class
        {
            // consider IoC here
            var entity = (T)Activator.CreateInstance(typeof(T));
            //set the value of the primary key (may error if wrong type)
            property.SetValue(entity, id, null);
            return entity;
        }

        private static EntityType GetElementType(this DbContext context, Type entityType)
        {
            throw new NotImplementedException();
            //var type = ObjectContext.GetObjectType(entityType);
            //var objectContext = ((IObjectContextAdapter)context).ObjectContext;

            //EntityType elementType;
            //if (objectContext.MetadataWorkspace.TryGetItem(type.FullName, DataSpace.OSpace, out elementType))
            //{
            //    return elementType;
            //}
            //return null;
        }

        /// <summary>
        /// Returns all navigation properties of the given <paramref name="entityType"/>.
        /// </summary>
        /// <param name="context">The context in which the <paramref name="entityType"/> lives.</param>
        /// <param name="entityType">The entity type for which this method returns the navigation properties.</param>
        /// <returns>A list of navigation properties.</returns>
        public static List<PropertyInfo> GetNavigationProperties(this DbContext context, Type entityType)
        {
            throw new NotImplementedException();
            //var elementType = context.GetElementType(entityType);
            //return elementType.NavigationProperties
            //    .Select(navigationProperty => entityType.GetProperty(navigationProperty.Name))
            //    .ToList();
        }

        /// <summary>
        /// Returns all navigation properties of the given <typeparamref name="TEntityType"/>.
        /// </summary>
        /// <param name="context">The context in which the <typeparamref name="TEntityType"/> lives.</param>
        /// <returns>A list of navigation properties.</returns>
        public static List<PropertyInfo> GetNavigationProperties<TEntityType>(this DbContext context) where TEntityType : class
        {
            var entityType = typeof(TEntityType);
            return context.GetNavigationProperties(entityType);
        }

        /// <summary>
        /// Returns the number of table rows per database table.
        /// </summary>
        public static List<TableRowCounts> GetTableRowCounts(this DbContext c)
        {
            throw new NotImplementedException();
            //var rawSqlQuery = c.Database.SqlQuery<TableRowCounts>(
            //    @"CREATE TABLE #counts
            //        (
            //            TableName varchar(255),
            //            TableRowCount int
            //        )

            //        EXEC sp_MSForEachTable @command1='INSERT #counts (TableName, TableRowCount) SELECT ''?'', COUNT(*) FROM ?'
            //        SELECT TableName, TableRowCount FROM #counts ORDER BY TableName, TableRowCount DESC
            //        DROP TABLE #counts");

            //var tableCountResults = rawSqlQuery.ToList();
            //return tableCountResults;
        }

        public static IQueryable Set(this DbContext context, Type T)
        {

            // Get the generic type definition
            MethodInfo method = typeof(DbContext).GetRuntimeMethod(nameof(DbContext.Set), null);

            // Build a method with the specific type argument you're interested in
            method = method.MakeGenericMethod(T);

            return method.Invoke(context, null) as IQueryable;
        }

        public static IQueryable<T> Set<T>(this DbContext context)
        {
            // Get the generic type definition 
            MethodInfo method = typeof(DbContext).GetRuntimeMethod(nameof(DbContext.Set), null);

            // Build a method with the specific type argument you're interested in 
            method = method.MakeGenericMethod(typeof(T));

            return method.Invoke(context, null) as IQueryable<T>;
        }
    }
}