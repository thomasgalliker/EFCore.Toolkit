using System;
using EFCore.Toolkit;
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
    public class TestAuditDbContext : AuditDbContextBase
    {
        public DbSet<TestEntity> TestEntities { get; set; }

        public DbSet<TestEntityAudit> TestEntityAudits { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<EmployeeAudit> EmployeeAudits { get; set; }

        public TestAuditDbContext(DbContextOptions dbContextOptions, IDatabaseInitializer databaseInitializer)
            : base(dbContextOptions, databaseInitializer)
        {
        }

        public TestAuditDbContext(DbContextOptions dbContextOptions, IDatabaseInitializer databaseInitializer, Action<string> log)
            : base(dbContextOptions, databaseInitializer, log)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PersonEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeAuditEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TestEntityEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TestEntityAuditEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new StudentEntityConfiguration());
            modelBuilder.ApplyConfiguration(new DepartmentEntityConfiguration());
            modelBuilder.ApplyConfiguration(new RoomEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CountryEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationSettingEntityTypeConfiguration());
        }
    }
}