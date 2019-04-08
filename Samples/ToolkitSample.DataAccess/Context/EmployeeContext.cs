using System;
using EFCore.Toolkit;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Auditing;
using EFCore.Toolkit.Auditing.Extensions;
using EFCore.Toolkit.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ToolkitSample.DataAccess.Context
{
    public class EmployeeContext : AuditDbContextBase<EmployeeContext>, IEmployeeContext
    {
        private static readonly AuditDbContextConfiguration AuditDbContextConfiguration = new AuditDbContextConfiguration(auditEnabled: true, auditDateTimeKind: DateTimeKind.Utc);

        /// <summary>
        ///     Empty constructor is used for 'update-database' command-line command.
        /// </summary>
        public EmployeeContext()
        {
        }

        public EmployeeContext(IDbConnection dbConnection, IDatabaseInitializer<EmployeeContext> initializer)
          : base(dbConnection, initializer, null)
        {
            this.ConfigureAuditing(AuditDbContextConfiguration);
        }

        public EmployeeContext(IDbConnection dbConnection, Action<string> log)
           : base(dbConnection, null, log)
        {
            this.ConfigureAuditing(AuditDbContextConfiguration);
        }

        public EmployeeContext(IDbConnection dbConnection, IDatabaseInitializer<EmployeeContext> initializer, Action<string> log = null)
           : base(dbConnection, initializer, log)
        {
            this.ConfigureAuditing(AuditDbContextConfiguration);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            this.Database.KillConnectionsToTheDatabase();

            modelBuilder.AddConfiguration(new PersonEntityConfiguration());
            modelBuilder.AddConfiguration(new EmployeeEntityTypeConfiguration());
            modelBuilder.AddConfiguration(new EmployeeAuditEntityTypeConfiguration());
            modelBuilder.AddConfiguration(new StudentEntityConfiguration());
            modelBuilder.AddConfiguration(new DepartmentEntityConfiguration());
            modelBuilder.AddConfiguration(new RoomEntityTypeConfiguration());
            modelBuilder.AddConfiguration(new CountryEntityConfiguration());
            modelBuilder.AddConfiguration(new ApplicationSettingEntityTypeConfiguration());

            //this.AutoConfigure(modelBuilder);
            //modelBuilder.Configurations.AddFromAssembly(this.GetType().Assembly);
        }
    }
}