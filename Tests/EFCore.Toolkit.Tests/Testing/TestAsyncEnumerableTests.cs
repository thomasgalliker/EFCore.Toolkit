using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFCore.Toolkit.Testing;
using EFCore.Toolkit.Tests.Stubs;
using FluentAssertions;
using Moq;
using ToolkitSample.DataAccess.Contracts.Repository;
using ToolkitSample.Model;
using Xunit;

namespace EFCore.Toolkit.Tests.Testing
{
    public class TestAsyncEnumerableTests
    {
        [Fact]
        public void ShouldReturnListAsTestAsyncEnumerable()
        {
            // Arrange
            var persons = new List<Person>
            {
                Testdata.Employees.CreateEmployee1(),
                Testdata.Employees.CreateEmployee2(),
                Testdata.Employees.CreateEmployee3()
            };

            var queryablePersons = new TestAsyncEnumerable<Person>(persons);

            var personRepositoryMock = new Mock<IPersonRepository>();
            personRepositoryMock.Setup(p => p.Get())
                .Returns(queryablePersons);

            // Act
            var mockedPersons = personRepositoryMock.Object.Get();

            // Assert
            mockedPersons.Should().NotBeNullOrEmpty();
            mockedPersons.Should().HaveCount(persons.Count);
        }
    }
}
