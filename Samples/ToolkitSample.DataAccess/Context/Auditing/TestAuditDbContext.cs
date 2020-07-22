using System;
using EFCore.Toolkit;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Auditing;
using Microsoft.EntityFrameworkCore;
using ToolkitSample.Model;
using ToolkitSample.Model.Auditing;

namespace ToolkitSample.DataAccess.Context.Auditing
{
    /// <summary>
    /// This data context is used to demonstrate the auditing features.
    /// It is configured using the app.config.
    /// </summary>
    public class TestAuditDbContext : AuditDbContextBase<TestAuditDbContext>
    {
        public DbSet<TestEntity> TestEntities { get; set; }

        public DbSet<TestEntityAudit> TestEntityAudits { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<EmployeeAudit> EmployeeAudits { get; set; }

        public TestAuditDbContext(DbContextOptions dbContextOptions, IDatabaseInitializer<TestAuditDbContext> databaseInitializer)
            : base(dbContextOptions, databaseInitializer)
        {
            //TODO this.Configuration.ProxyCreationEnabled = false;
            this.ConfigureAuditingFromAppConfig();
        }

        public TestAuditDbContext(DbContextOptions dbContextOptions, IDatabaseInitializer<TestAuditDbContext> databaseInitializer, Action<string> log)
            : base(dbContextOptions, databaseInitializer, log)
        {
            //TODO this.Configuration.ProxyCreationEnabled = false;
            this.ConfigureAuditingFromAppConfig();
        }

        public TestAuditDbContext(IDbConnection dbConnection, IDatabaseInitializer<TestAuditDbContext> databaseInitializer, Action<string> log = null)
            : base(dbConnection, databaseInitializer, log)
        {
            //TODO this.Configuration.ProxyCreationEnabled = false;
            this.ConfigureAuditingFromAppConfig();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PersonEntityConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeAuditEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TestEntityEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TestEntityAuditEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new StudentEntityConfiguration());
            modelBuilder.ApplyConfiguration(new DepartmentEntityConfiguration());
            modelBuilder.ApplyConfiguration(new RoomEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CountryEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationSettingEntityTypeConfiguration());
        }
    }
}