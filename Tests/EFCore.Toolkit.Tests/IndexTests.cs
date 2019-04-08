using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Testing;
using EFCore.Toolkit.Tests.Auditing;
using EFCore.Toolkit.Tests.Extensions;
using EFCore.Toolkit.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ToolkitSample.DataAccess.Context;
using ToolkitSample.Model;

using Xunit;
using Xunit.Abstractions;

namespace EFCore.Toolkit.Tests
{
    public class IndexTests : ContextTestBase<EmployeeContext>
    {
        public IndexTests(ITestOutputHelper testOutputHelper)
            : base(dbConnection: () => new EmployeeContextTestDbConnection(),
                  databaseInitializer: new CreateDatabaseIfNotExists<EmployeeContext>(),
                  log: testOutputHelper.WriteLine)
        {
            AssemblyLoader.Current = new TestAssemblyLoader();
        }

        [Fact]
        public void ShouldAddRoomsIfMultipleColumnsAreUnique()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room { Level = 1, Sector = "A" },
                new Room { Level = 1, Sector = "B" },
                new Room { Level = 1, Sector = "C" },
            };
            ChangeSet committedChangeSet;

            // Act
            using (var context = this.CreateContext())
            {
                context.Set<Room>().Add(rooms[0]);
                context.Set<Room>().Add(rooms[1]);
                context.Set<Room>().Add(rooms[2]);
                committedChangeSet = context.SaveChanges();
            }

            // Assert
            committedChangeSet.Assert(expectedNumberOfAdded: 3, expectedNumberOfModified: 0, expectedNumberOfDeleted: 0);

            using (var context = this.CreateContext())
            {
                var allRooms = context.Set<Room>().ToList();
                allRooms.Should().HaveCount(3);
            }
        }

        [Fact]
        public void ShouldThrowExceptionIfIfMultipleColumnsAreNotUnique()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room { Level = 1, Sector = "A" },
                new Room { Level = 1, Sector = "A" },
            };

            // Act
            using (var context = this.CreateContext())
            {
                context.Set<Room>().Add(rooms[0]);
                context.Set<Room>().Add(rooms[1]);

                Action action = () => context.SaveChanges();

                // Assert
                var ex = action.Should().Throw<DbUpdateException>();
                ex.Which.InnerException.InnerException.Message.Should()
                    .Contain("Cannot insert duplicate key row in object 'dbo.Room' with unique index 'IX_Room_Level_Sector'. The duplicate key value is (1, A).");
            }
        }
    }
}