using System;
using EntityFramework.Toolkit.EFCore;
using EntityFramework.Toolkit.EFCore.Auditing;
using EntityFramework.Toolkit.EFCore.Auditing.Extensions;
using EntityFramework.Toolkit.EFCore.Contracts;
using EntityFramework.Toolkit.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;
using ToolkitSample.Model;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            this.Database.KillConnectionsToTheDatabase();

            modelBuilder.AddConfiguration(new PersonEntityConfiguration<Person>());
            modelBuilder.AddConfiguration(new EmployeeEntityTypeConfiguration());
            modelBuilder.AddConfiguration(new EmployeeAuditEntityTypeConfiguration());
            modelBuilder.AddConfiguration(new StudentEntityConfiguration());
            modelBuilder.AddConfiguration(new DepartmentEntityConfiguration());
            modelBuilder.AddConfiguration(new RoomConfiguration());
            modelBuilder.AddConfiguration(new CountryEntityConfiguration());
            modelBuilder.AddConfiguration(new ApplicationSettingEntityTypeConfiguration());

            //this.AutoConfigure(modelBuilder);
            //modelBuilder.Configurations.AddFromAssembly(this.GetType().Assembly);
        }
    }
}