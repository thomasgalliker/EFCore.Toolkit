using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Exceptions;
using EFCore.Toolkit.Testing;
using EFCore.Toolkit.Tests.Auditing;
using EFCore.Toolkit.Tests.Stubs;
using EFCore.Toolkit.Utils;
using FluentAssertions;

using Moq;

using ToolkitSample.DataAccess.Context;
using ToolkitSample.Model;

using Xunit;
using Xunit.Abstractions;

namespace EFCore.Toolkit.Tests
{
    public class UnitOfWorkIntegrationTests : ContextTestBase<EmployeeContext, EmployeeContextTestDbConnection>
    {
        //public UnitOfWorkIntegrationTests(ITestOutputHelper testOutputHelper)
        //    : base(dbConnection: () => new EmployeeContextTestDbConnection(),
        //           log: testOutputHelper.WriteLine)
        //{
        //    AssemblyLoader.Current = new TestAssemblyLoader();
        //}

        public UnitOfWorkIntegrationTests(ITestOutputHelper testOutputHelper)
            : base(databaseInitializer: new CreateDatabaseIfNotExists<EmployeeContext>(),
                log: testOutputHelper.WriteLine)
        {
            AssemblyLoader.Current = new TestAssemblyLoader();
        }

        [Fact]
        public void ShouldCommitToMultipleContexts()
        {
            // Arrange
            ICollection<ChangeSet> changeSets;
            using (IUnitOfWork unitOfWork = new UnitOfWork())
            {
                var context1 = this.CreateContext();
                var contextMock2 = new Mock<ISampleContextTwo>();
                contextMock2.Setup(m => m.SaveChanges()).Returns(new ChangeSet(typeof(ISampleContextTwo), new List<IChange> { Change.CreateAddedChange(new Person()) }));
                var context2 = contextMock2.Object;

                context1.Set<Employee>().Add(Testdata.Employees.CreateEmployee1());

                unitOfWork.RegisterContext(context1);
                unitOfWork.RegisterContext(context2);

                // Act
                changeSets = unitOfWork.Commit();
            }

            // Assert
            changeSets.Should().HaveCount(2);

            var assertContext1 = this.CreateContext();
            assertContext1.Set<Employee>().ToList().Should().HaveCount(1);
        }

        [Fact]
        public void ShouldFailToCommitMultipleContexts()
        {
            // Arrange
            IUnitOfWork unitOfWork = new UnitOfWork();

            var context1 = this.CreateContext(databaseInitializer: new DropCreateDatabaseAlways<EmployeeContext>());
            var context2 = new Mock<ISampleContextTwo>();
            context2.Setup(m => m.SaveChanges()).Throws(new InvalidOperationException("SampleContextTwo failed to SaveChanges."));

            context1.Set<Employee>().Add(Testdata.Employees.CreateEmployee1());

            unitOfWork.RegisterContext(context1);
            unitOfWork.RegisterContext(context2.Object);

            // Act
            Action action = () => unitOfWork.Commit();

            // Assert
            var ex = action.Should().Throw<UnitOfWorkException>();
            ex.Which.Message.Should().Contain("failed to commit.");
            ex.WithInnerException<InvalidOperationException>();
            ex.Which.InnerException.Message.Should().Contain("SampleContextTwo failed to SaveChanges.");

            var context = this.CreateContext(new DropCreateDatabaseAlways<EmployeeContext>());
            context.Set<Employee>().ToList().Should().HaveCount(0);
        }

        //TODO Write test to save + check summary of changes
        //TODO Write test to saveasync + check summary of changes
    }
}