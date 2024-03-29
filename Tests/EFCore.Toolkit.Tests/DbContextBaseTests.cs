﻿using System;
using System.Linq;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Concurrency;
using EFCore.Toolkit.Testing;
using EFCore.Toolkit.Tests.Auditing;
using EFCore.Toolkit.Tests.Stubs;
using EFCore.Toolkit.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ToolkitSample.DataAccess.Context;
using ToolkitSample.Model;

using Xunit;

namespace EFCore.Toolkit.Tests
{
    public class DbContextBaseTests : ContextTestBase<EmployeeContext>
    {
        public DbContextBaseTests()
            : base(dbContextOptions: EmployeeContextTestDbConnection.CreateDbContextOptions<EmployeeContext>())
        {
            AssemblyLoader.Current = new TestAssemblyLoader();
        }

        [Fact]
        public async void ShouldSaveChangesAsync()
        {
            // Arrange
            var initialEmployee = Testdata.Employees.CreateEmployee1();
            ChangeSet changeSet = null;

            // Act
            using (var employeeContext = this.CreateContext())
            {
                employeeContext.Set<Employee>().Add(initialEmployee);
                changeSet = await employeeContext.SaveChangesAsync();
            }

            // Assert
            changeSet.Should().NotBeNull();
            changeSet.Changes.Should().HaveCount(1);
            changeSet.Changes.Where(c => c.State == ChangeState.Added).Should().HaveCount(1);
        }

        [Fact]
        public void ShouldRethrowConcurrencyUpdateExceptionAsDefault()
        {
            // Arrange
            var databaseInitializer = new CreateDatabaseIfNotExists<EmployeeContext>();
            var initialEmployee = Testdata.Employees.CreateEmployee1();

            string firstNameChange1 = initialEmployee.FirstName + " from employeeContext1";
            string firstNameChange2 = initialEmployee.FirstName + " from employeeContext2";

            using (var employeeContext = this.CreateContext(databaseInitializer))
            {
                employeeContext.Set<Employee>().Add(initialEmployee);
                employeeContext.SaveChanges();
            }

            // Act
            using (var employeeContext1 = this.CreateContext(databaseInitializer))
            {
                // Get an employee (which has a Version Timestamp) in one context and modify
                var employeeFromContext1 = employeeContext1.Set<Employee>().First(p => p.Id == 1);
                employeeFromContext1.FirstName = firstNameChange1;

                // Modify and Save the same employee in another context to simulate concurrent access
                using (var employeeContext2 = this.CreateContext(databaseInitializer))
                {
                    var employeeFromContext2 = employeeContext2.Set<Employee>().First(p => p.Id == 1);
                    employeeFromContext2.FirstName = firstNameChange2;
                    employeeContext2.SaveChanges();
                }

                // SaveChanges of the first context results in a DbUpdateConcurrencyException
                Action action = () => employeeContext1.SaveChanges();

                // Assert
                action.Should().Throw<DbUpdateConcurrencyException>();
            }
        }

        [Fact]
        public void ShouldResolveConcurrencyExceptionWithDatabaseWinsStrategy()
        {
            // Arrange
            var databaseInitializer = new CreateDatabaseIfNotExists<EmployeeContext>();
            IConcurrencyResolveStrategy concurrencyResolveStrategy = new DatabaseWinsConcurrencyResolveStrategy();
            var initialEmployee = Testdata.Employees.CreateEmployee1();

            string firstNameChange1 = initialEmployee.FirstName + " from employeeContext1";
            string firstNameChange2 = initialEmployee.FirstName + " from employeeContext2";

            using (var employeeContext = this.CreateContext(databaseInitializer))
            {
                employeeContext.Set<Employee>().Add(initialEmployee);
                employeeContext.SaveChanges();
            }

            // Act
            using (var employeeContext1 = this.CreateContext(databaseInitializer))
            {
                employeeContext1.ConcurrencyResolveStrategy = concurrencyResolveStrategy;

                // Get an employee (which has a Version Timestamp) in one context and modify
                var employeeFromContext1 = employeeContext1.Set<Employee>().First(p => p.Id == 1);
                employeeFromContext1.FirstName = firstNameChange1;

                // Modify and Save the same employee in another context to simulate concurrent access
                using (var employeeContext2 = this.CreateContext(databaseInitializer))
                {
                    var employeeFromContext2 = employeeContext2.Set<Employee>().First(p => p.Id == 1);
                    employeeFromContext2.FirstName = firstNameChange2;
                    employeeContext2.SaveChanges();
                }

                // SaveChanges of the first context results in a DbUpdateConcurrencyException
                employeeContext1.SaveChanges();
            }

            // Assert
            using (var employeeContext = this.CreateContext(databaseInitializer))
            {
                var returnedEmployee = employeeContext.Set<Employee>().First(p => p.Id == 1);
                returnedEmployee.FirstName.Should().Be(firstNameChange2);
            }
        }

        [Fact]
        public void ShouldResolveConcurrencyExceptionWithClientWinsStrategy()
        {
            // Arrange
            var databaseInitializer = new CreateDatabaseIfNotExists<EmployeeContext>();
            IConcurrencyResolveStrategy concurrencyResolveStrategy = new ClientWinsConcurrencyResolveStrategy();
            var initialEmployee = Testdata.Employees.CreateEmployee1();

            string firstNameChange1 = initialEmployee.FirstName + " from employeeContext1";
            string firstNameChange2 = initialEmployee.FirstName + " from employeeContext2";

            using (var employeeContext = this.CreateContext(databaseInitializer))
            {
                employeeContext.Set<Employee>().Add(initialEmployee);
                employeeContext.SaveChanges();
            }

            // Act
            using (var employeeContext1 = this.CreateContext(databaseInitializer))
            {
                employeeContext1.ConcurrencyResolveStrategy = concurrencyResolveStrategy;

                // Get an employee (which has a Version Timestamp) in one context and modify
                var employeeFromContext1 = employeeContext1.Set<Employee>().First(p => p.Id == 1);
                employeeFromContext1.FirstName = firstNameChange1;

                // Modify and Save the same employee in another context to simulate concurrent access
                using (var employeeContext2 = this.CreateContext(databaseInitializer))
                {
                    var employeeFromContext2 = employeeContext2.Set<Employee>().First(p => p.Id == 1);
                    employeeFromContext2.FirstName = firstNameChange2;
                    employeeContext2.SaveChanges();
                }

                // SaveChanges of the first context results in a DbUpdateConcurrencyException
                employeeContext1.SaveChanges();
            }

            // Assert
            using (var employeeContext = this.CreateContext(databaseInitializer))
            {
                var returnedEmployee = employeeContext.Set<Employee>().First(p => p.Id == 1);
                returnedEmployee.FirstName.Should().Be(firstNameChange1);
            }
        }
    }
}