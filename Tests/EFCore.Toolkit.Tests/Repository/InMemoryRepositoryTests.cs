using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EFCore.Toolkit.Tests.Stubs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ToolkitSample.DataAccess.Contracts.Repository;
using ToolkitSample.Model;
using Xunit;

namespace EFCore.Toolkit.Tests.Repository
{
    public class InMemoryRepositoryTests
    {
        [Fact]
        public void ShouldAddPerson()
        {
            // Arrange
            var employee1 = Testdata.Employees.CreateEmployee1();
            var personRepository = new InMemoryPersonRepository();

            // Act
            personRepository.Add(employee1);

            // Assert
            var allPersons = personRepository.GetAll();
            allPersons.Should().HaveCount(1);
        }

        [Fact]
        public async Task ShouldGetToListAsync()
        {
            // Arrange
            var personRepository = new InMemoryPersonRepository();

            personRepository.Add(Testdata.Employees.CreateEmployee1());
            personRepository.Add(Testdata.Employees.CreateEmployee2());
            personRepository.Add(Testdata.Employees.CreateEmployee3());

            // Act
            var persons = await personRepository.Get().ToListAsync();

            // Assert
            persons.Should().HaveCount(3);
        }
    }

    public class InMemoryPersonRepository : InMemoryRepository<Person>, IPersonRepository
    {
    }
}
