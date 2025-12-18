using EFCore.Toolkit.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit.Testing
{
    /// <summary>
    /// ContextTestBase for context <typeparamref name="TContext"/> using IDbConnection <typeparamref name="TDbContextOptionsBuilder"/>
    /// </summary>
    /// <typeparam name="TContext">The database context.</typeparam>
    /// <typeparam name="TDbContextOptionsBuilder">The database connection.</typeparam>
    public abstract class ContextTestBase<TContext, TDbContextOptionsBuilder> : ContextTestBase<TContext>
        where TContext : DbContextBase where TDbContextOptionsBuilder : DbContextOptionsBuilder, new()
    {
        protected ContextTestBase() : base(new TDbContextOptionsBuilder().Options)
        {
        }

        protected ContextTestBase(bool deleteDatabaseOnDispose) : base(new TDbContextOptionsBuilder().Options, deleteDatabaseOnDispose)
        {
        }

        protected ContextTestBase(Action<string> log) : base(new TDbContextOptionsBuilder().Options, log)
        {
        }

        protected ContextTestBase(Action<string> log, bool deleteDatabaseOnDispose) : base(new TDbContextOptionsBuilder().Options, log, deleteDatabaseOnDispose)
        {
        }

        protected ContextTestBase(IDatabaseInitializer databaseInitializer) : base(new TDbContextOptionsBuilder().Options, databaseInitializer)
        {
        }

        protected ContextTestBase(IDatabaseInitializer databaseInitializer, Action<string> log) : base(new TDbContextOptionsBuilder().Options, databaseInitializer, log)
        {
        }

        protected ContextTestBase(IDatabaseInitializer databaseInitializer, Action<string> log, bool deleteDatabaseOnDispose) : base(new TDbContextOptionsBuilder().Options, databaseInitializer, log, deleteDatabaseOnDispose)
        {
        }
    }

    public abstract class ContextTestBase<TContext> : IDisposable
        where TContext : DbContextBase
    {
        private readonly ICollection<TContext> contextInstances = new List<TContext>();
        private readonly DbContextOptions dbContextOptions;
        private readonly IDatabaseInitializer? databaseInitializer;
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

        protected ContextTestBase(DbContextOptions dbContextOptions, IDatabaseInitializer? databaseInitializer)
            : this(dbContextOptions: dbContextOptions, databaseInitializer: databaseInitializer, log: null)
        {
        }

        protected ContextTestBase(DbContextOptions dbContextOptions, IDatabaseInitializer? databaseInitializer, Action<string>? log)
            : this(dbContextOptions: dbContextOptions, databaseInitializer: databaseInitializer, log: log, deleteDatabaseOnDispose: true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTestBase{TContext}" /> class.
        /// </summary>
        /// <param name="dbContextOptions">The <see cref="DbContextOptions" /> which is used to connect to the database.</param>
        /// <param name="log">Log delegate used to write diagnostic log messages to.</param>
        /// <param name="databaseInitializer">
        /// The <see cref="IDatabaseInitializer{TContext}" /> which is used initialize the
        /// database. (Default is <see cref="DropCreateDatabaseAlways{TContext}" />).
        /// </param>
        /// <param name="deleteDatabaseOnDispose">Determines if the database needs to be deleted on dispose. (Default is true).</param>
        protected ContextTestBase(DbContextOptions dbContextOptions, IDatabaseInitializer? databaseInitializer, Action<string>? log, bool deleteDatabaseOnDispose)
        {
            this.dbContextOptions = dbContextOptions;
            this.Log = log;
            this.DeleteDatabaseOnDispose = deleteDatabaseOnDispose;
            this.databaseInitializer = databaseInitializer;
        }

        public Action<string>? Log { get; set; }

        protected bool DeleteDatabaseOnDispose { get; set; }

        /// <summary>
        /// Returns the default database initializer (given by ctor) if <paramref name="databaseInitializer" /> is null.
        /// </summary>
        private IDatabaseInitializer EnsureDatabaseInitializer(IDatabaseInitializer? databaseInitializer)
        {
            if (databaseInitializer == null)
            {
                databaseInitializer = this.databaseInitializer ?? new DropCreateDatabaseAlways();
            }

            return databaseInitializer;
        }

        /// <summary>
        /// Returns the default db connection (given by ctor) if <paramref name="dbContextOptions" /> is null.
        /// </summary>
        private DbContextOptions EnsureDbContextOptions(DbContextOptions dbContextOptions)
        {
            if (dbContextOptions == null)
            {
                dbContextOptions = this.dbContextOptions;
            }

            return dbContextOptions;
        }

        protected TContext CreateContext()
        {
            return this.CreateContext(this.databaseInitializer);
        }

        protected TContext CreateContext(IDatabaseInitializer? databaseInitializer = null)
        {
            var args = new List<object>();

            var dbContextOptions = this.EnsureDbContextOptions(this.dbContextOptions);
            args.Add(dbContextOptions);

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
                            using (var context = this.CreateContext(new CreateDatabaseIfNotExists()))
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