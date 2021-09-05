using System;
using EFCore.Toolkit;
using EFCore.Toolkit.Auditing;
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

        public EmployeeContext(DbContextOptions dbContextOptions, IDatabaseInitializer<EmployeeContext> initializer)
          : base(dbContextOptions, initializer, null)
        {
            this.ConfigureAuditing(AuditDbContextConfiguration);
        }

        public EmployeeContext(DbContextOptions dbContextOptions, Action<string> log)
           : base(dbContextOptions, null, log)
        {
            this.ConfigureAuditing(AuditDbContextConfiguration);
        }

        public EmployeeContext(DbContextOptions dbContextOptions, IDatabaseInitializer<EmployeeContext> initializer, Action<string> log = null)
           : base(dbContextOptions, initializer, log)
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

            //this.Database.KillConnectionsToTheDatabase();

            modelBuilder.ApplyConfiguration(new PersonEntityConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeAuditEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new StudentEntityConfiguration());
            modelBuilder.ApplyConfiguration(new DepartmentEntityConfiguration());
            modelBuilder.ApplyConfiguration(new RoomEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CountryEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationSettingEntityTypeConfiguration());

            //this.AutoConfigure(modelBuilder);
            //modelBuilder.Configurations.AddFromAssembly(this.GetType().Assembly);
        }
    }
}