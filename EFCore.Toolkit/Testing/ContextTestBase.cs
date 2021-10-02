using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit.Testing
{
    /// <summary>
    /// ContextTestBase for context <typeparam name="TContext"/> using IDbConnection <typeparam name="TDbConnection"/>
    /// </summary>
    /// <typeparam name="TContext">The database context.</typeparam>
    /// <typeparam name="TDbConnection">The database connection.</typeparam>
    public abstract class ContextTestBase<TContext, TDbConnection> : ContextTestBase<TContext>
        where TContext : DbContextBase<TContext> where TDbConnection : DbContextOptionsBuilder, new()
    {
        protected ContextTestBase() : base(new TDbConnection().Options)
        {
        }

        protected ContextTestBase(bool deleteDatabaseOnDispose) : base(new TDbConnection().Options, deleteDatabaseOnDispose)
        {
        }

        protected ContextTestBase(Action<string> log) : base(new TDbConnection().Options, log)
        {
        }

        protected ContextTestBase(Action<string> log, bool deleteDatabaseOnDispose) : base(new TDbConnection().Options, log, deleteDatabaseOnDispose)
        {
        }

        protected ContextTestBase(IDatabaseInitializer<TContext> databaseInitializer) : base(new TDbConnection().Options, databaseInitializer)
        {
        }

        protected ContextTestBase(IDatabaseInitializer<TContext> databaseInitializer, Action<string> log) : base(new TDbConnection().Options, databaseInitializer, log)
        {
        }

        protected ContextTestBase(IDatabaseInitializer<TContext> databaseInitializer, Action<string> log, bool deleteDatabaseOnDispose) : base(new TDbConnection().Options, databaseInitializer, log, deleteDatabaseOnDispose)
        {
        }
    }

    public abstract class ContextTestBase<TContext> : IDisposable
        where TContext : DbContextBase<TContext>
    {
        private readonly ICollection<TContext> contextInstances = new List<TContext>();
        private readonly DbContextOptions dbContextOptions;
        private readonly IDatabaseInitializer<TContext> databaseInitializer;
        private bool disposed;

        protected ContextTestBase(DbContextOptions dbContextOptions)
            : this(dbContextOptions: dbContextOptions, databaseInitializer: null, log: null)
        {
        }

        protected ContextTestBase(DbContextOptions dbContextOptions, bool deleteDatabaseOnDispose)
            : this(dbContextOptions: dbContextOptions, databaseInitializer: null, log: null, deleteDatabaseOnDispose: deleteDatabaseOnDispose)
        {
        }

        protected ContextTestBase(DbContextOptions dbContextOptions, Action<string> log)
            : this(dbContextOptions: dbContextOptions, databaseInitializer: null, log: log, deleteDatabaseOnDispose: true)
        {
        }

        protected ContextTestBase(DbContextOptions dbContextOptions, Action<string> log, bool deleteDatabaseOnDispose)
            : this(dbContextOptions: dbContextOptions, databaseInitializer: null, log: log, deleteDatabaseOnDispose: deleteDatabaseOnDispose)
        {
        }

        protected ContextTestBase(DbContextOptions dbContextOptions, IDatabaseInitializer<TContext> databaseInitializer)
            : this(dbContextOptions: dbContextOptions, databaseInitializer: databaseInitializer, log: null)
        {
        }

        protected ContextTestBase(DbContextOptions dbContextOptions, IDatabaseInitializer<TContext> databaseInitializer, Action<string> log)
            : this(dbContextOptions: dbContextOptions, databaseInitializer: databaseInitializer, log: log, deleteDatabaseOnDispose: true)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ContextTestBase{TContext}" /> class.
        /// </summary>
        /// <param name="dbContextOptions">The <see cref="IDbConnection" /> which is used to connect to the database.</param>
        /// <param name="log">Log delegate used to write diagnostic log messages to.</param>
        /// <param name="databaseInitializer">
        ///     The <see cref="IDatabaseInitializer{TContext}" /> which is used initialize the
        ///     database. (Default is <see cref="DropCreateDatabaseAlways{TContext}" />).
        /// </param>
        /// <param name="deleteDatabaseOnDispose">Determines if the database needs to be deleted on dispose. (Default is true).</param>
        protected ContextTestBase(DbContextOptions dbContextOptions, IDatabaseInitializer<TContext> databaseInitializer, Action<string> log, bool deleteDatabaseOnDispose)
        {
            this.dbContextOptions = dbContextOptions;
            this.Log = log;
            this.DeleteDatabaseOnDispose = deleteDatabaseOnDispose;
            this.databaseInitializer = databaseInitializer;
        }

        public Action<string> Log { get; set; }

        protected bool DeleteDatabaseOnDispose { get; set; }

        /// <summary>
        ///     Returns the default database initializer (given by ctor) if <paramref name="databaseInitializer" /> is null.
        /// </summary>
        private IDatabaseInitializer<TContext> EnsureDatabaseInitializer(IDatabaseInitializer<TContext> databaseInitializer)
        {
            if (databaseInitializer == null)
            {
                databaseInitializer = this.databaseInitializer ?? new DropCreateDatabaseAlways<TContext>();
            }

            return databaseInitializer;
        }

        /// <summary>
        ///     Returns the default db connection (given by ctor) if <paramref name="dbContextOptions" /> is null.
        /// </summary>
        private DbContextOptions EnsureDbConnection(DbContextOptions dbContextOptions)
        {
            if (dbContextOptions == null)
            {
                dbContextOptions = this.dbContextOptions;
            }

            if (dbContextOptions == null && string.IsNullOrEmpty(this.dbContextOptionsString))
            {
                throw new InvalidOperationException("Either dbContextOptions or nameOrConnectionString must be defined.");
            }

            //if (dbContextOptions == null)
            //{
            //    dbContextOptions = new DbConnection(this.dbContextOptionsString);
            //}

            return dbContextOptions;
        }

        protected TContext CreateContext()
        {
            return this.CreateContext(this.databaseInitializer);
        }

        protected TContext CreateContext(IDatabaseInitializer<TContext> databaseInitializer = null)
        {
            var args = new List<object>();
            if (!string.IsNullOrEmpty(this.dbContextOptionsString))
            {
                args.Add(this.dbContextOptionsString);
            }
            else
            {
                var dbConn = this.EnsureDbConnection(this.dbContextOptions);
                args.Add(dbConn);
            }

            if (databaseInitializer == null)
            {
                databaseInitializer = this.EnsureDatabaseInitializer(this.databaseInitializer);
            }
            args.Add(databaseInitializer);

            if (this.Log != null)
            {
                args.Add(this.Log);
            }

            return this.CreateContext(args.ToArray());
        }

        protected TContext CreateContext(params object[] args)
        {
            var contextType = typeof(TContext);
            var context = CreateContextInstance(contextType, args);

            this.contextInstances.Add(context);
            return context;
        }

        private static TContext CreateContextInstance(Type contextType, params object[] args)
        {
            var contextCtor = contextType.GetMatchingConstructor(args);
            return (TContext)contextCtor.Invoke();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.DeleteDatabaseOnDispose)
                {
                    if (this.contextInstances.Any())
                    {
                        // Drop & dispose all created context instances (if they've not already been disposed during test execution)
                        var dropped = false;
                        foreach (var context in this.contextInstances.Where(c => !c.IsDisposed))
                        {
                            if (dropped == false)
                            {
                                context.DropDatabase();
                                dropped = true;
                            }
                            context.Dispose();
                        }

                        // If all contexts have already been disposed, create a new context in order to drop the database
                        if (dropped == false)
                        {
                            using (var context = this.CreateContext(new CreateDatabaseIfNotExists<TContext>()))
                            {
                                context.DropDatabase();
                            }
                        }
                    }
                }

                this.contextInstances.Clear();
            }

            this.disposed = true;
        }
    }
}