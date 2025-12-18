using System;
using EFCore.Toolkit;
using Microsoft.EntityFrameworkCore;

namespace ToolkitSample.DataAccess.Context
{
    public class EmployeeContext : DbContextBase, IEmployeeContext
    {
        /// <summary>
        /// Empty constructor is used for 'update-database' command-line command.
        /// </summary>
        public EmployeeContext()
        {
        }

        public EmployeeContext(DbContextOptions dbContextOptions, IDatabaseInitializer initializer)
          : base(dbContextOptions, initializer, null)
        {
        }

        public EmployeeContext(DbContextOptions dbContextOptions, Action<string> log)
           : base(dbContextOptions, null, log)
        {
        }

        public EmployeeContext(DbContextOptions dbContextOptions, IDatabaseInitializer initializer, Action<string>? log = null)
           : base(dbContextOptions, initializer, log)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //this.Database.KillConnectionsToTheDatabase();

            modelBuilder.ApplyConfiguration(new PersonEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeAuditEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new StudentEntityConfiguration());
            modelBuilder.ApplyConfiguration(new DepartmentEntityConfiguration());
            modelBuilder.ApplyConfiguration(new RoomEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CountryEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationSettingEntityTypeConfiguration());

            //this.AutoConfigure(modelBuilder);
            //modelBuilder.Configurations.AddFromAssembly(this.GetType().Assembly);
        }
    }
}