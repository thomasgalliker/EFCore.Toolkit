﻿using System.Collections.Generic;
using System.Linq;

namespace EFCore.Toolkit.Abstractions
{
    public interface IReadOnlyRepository<T> : IRepository
    {
        /// <summary>
        ///     Returns a collection of all entities in the context, or that can be queried from the
        ///     database, of given type <typeparamref name="T" />. IQueryable enables database-spezific filtering.
        /// </summary>
        IQueryable<T> Get();

        /// <summary>
        ///     Returns a collection of all entities in the context, or that can be queried from the
        ///     database, of given type <typeparamref name="T" />.
        /// </summary>
        IEnumerable<T> GetAll();

        /// <summary>
        ///     Finds an entity with the given primary key values.
        ///     If an entity with the given primary key values exists in the context, then it is
        ///     returned immediately without making a request to the store.  Otherwise, a request
        ///     is made to the store for an entity with the given primary key values and this entity,
        ///     if found, is attached to the context and returned.  If no entity is found in the
        ///     context or the store, then null is returned.
        /// </summary>
        /// <param name="ids">The values of the primary key for the entity to be found. </param>
        /// <returns> The entity found, or null. </returns>
        T FindById(params object[] ids);
    }
}