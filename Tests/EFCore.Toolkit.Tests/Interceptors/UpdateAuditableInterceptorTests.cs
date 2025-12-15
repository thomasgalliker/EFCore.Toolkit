using System;
using System.Linq;
using System.Threading.Tasks;
using EFCore.Toolkit.Testing;
using EFCore.Toolkit.Tests.Auditing;
using EFCore.Toolkit.Tests.Stubs;
using EFCore.Toolkit.Utils;
using FluentAssertions;
using ToolkitSample.DataAccess.Context;
using ToolkitSample.Model;
using Xunit;
using Xunit.Abstractions;

namespace EFCore.Toolkit.Tests.Interceptors
{
    public class UpdateAuditableInterceptorTests : ContextTestBase<EmployeeContext, EmployeeContextTestDbConnection<EmployeeContext>>
    {
        public UpdateAuditableInterceptorTests(ITestOutputHelper testOutputHelper)
             : base(databaseInitializer: new CreateDatabaseIfNotExists(),
                   log: testOutputHelper.WriteLine)
        {
            AssemblyLoader.Current = new TestAssemblyLoader();
        }

        [Fact]
        public async Task ShouldAuditCreatedAndUpdatedDate()
        {
            // Arrange
            var initialEmployee = Testdata.Employees.CreateEmployee1();

            // Act
            using (var employeeContext = this.CreateContext())
            {
                var employeeSet = employeeContext.Set<Employee>();
                employeeSet.Add(initialEmployee);
                employeeContext.SaveChanges();

                await Task.Delay(1000);

                initialEmployee.FirstName = "Updated " + initialEmployee.FirstName;
                employeeContext.SaveChanges();
            }

            // Assert
            using (var employeeContext = this.CreateContext())
            {
                var employeeSet = employeeContext.Set<Employee>();
                var allEmployees = employeeSet.ToList();
                allEmployees.Where(e => e.CreatedDate > DateTime.MinValue).Should().HaveCount(1);
                allEmployees.Where(e => e.UpdatedDate > e.CreatedDate).Should().HaveCount(1);
            }
        }

        [Fact]
        public void ShouldNotUpdateAuditCreatedDate_ICreatedDateAndIUpdatedDate()
        {
            // Arrange
            var initialEmployee = Testdata.Employees.CreateEmployee1();
            using (var employeeContext = this.CreateContext())
            {
                var employeeSet = employeeContext.Set<Employee>();
                employeeSet.Add(initialEmployee);
                employeeContext.SaveChanges();
            }

            // Act
            using (var employeeContext = this.CreateContext())
            {
                var employeeSet = employeeContext.Set<Employee>();
                var updateEmployee = employeeSet.Find(initialEmployee.Id);
                updateEmployee.CreatedDate = DateTime.MinValue;
                employeeSet.Update(updateEmployee);
                employeeContext.SaveChanges();
            }

            // Assert
            using (var employeeContext = this.CreateContext())
            {
                var employeeSet = employeeContext.Set<Employee>();
                var allEmployees = employeeSet.ToList();
                allEmployees.Where(e => e.CreatedDate > DateTime.MinValue).Should().HaveCount(1);
                allEmployees.Where(e => e.UpdatedDate > e.CreatedDate).Should().HaveCount(1);
            }
        }

        [Fact]
        public void ShouldNotUpdateAuditCreatedDate_ICreatedDate()
        {
            // Arrange
            var initialRoom = Testdata.Rooms.GetRoom1B();
            using (var employeeContext = this.CreateContext())
            {
                var roomSet = employeeContext.Set<Room>();
                roomSet.Add(initialRoom);
                employeeContext.SaveChanges();
            }

            // Act
            var manipulatedCreatedDate = new DateTime(2000, 1, 1);
            using (var employeeContext = this.CreateContext())
            {
                var roomSet = employeeContext.Set<Room>();
                var updateRoom = roomSet.Find(initialRoom.Id);
                updateRoom.CreatedDate = manipulatedCreatedDate;
                roomSet.Update(updateRoom);
                employeeContext.SaveChanges();
            }

            // Assert
            using (var employeeContext = this.CreateContext())
            {
                var roomSet = employeeContext.Set<Room>();
                var allRooms = roomSet.ToList();
                var updatedRoom = allRooms.ElementAt(0);
                updatedRoom.CreatedDate.Should().NotBeCloseTo(manipulatedCreatedDate, precision: TimeSpan.FromSeconds(2));
            }
        }
    }
}
