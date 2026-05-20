using EFCore.Toolkit.Tests.Stubs;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using ToolkitSample.DataAccess.Contracts.Repository;
using ToolkitSample.Model;
using Xunit;
using Xunit.Abstractions;

namespace EFCore.Toolkit.Tests.Repositories
{
    [Trait(Traits.Category, Traits.UnitTests)]
    public class InMemoryRepositoryTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public InMemoryRepositoryTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

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
        public async Task ShouldAddPerson_Parallel()
        {
            // Arrange
            var personRepository = new InMemoryPersonRepository();

            // Act
            var tasks = Enumerable.Range(1, 100).Select(i => Task.Factory.StartNew(() =>
            {
                var employee = Testdata.Employees.CreateEmployee1();
                personRepository.Add(employee);
                this.testOutputHelper.WriteLine($"personRepository.Add(Id={employee.Id})");
            }));

            await Task.WhenAll(tasks);

            // Assert
            var allPersons = personRepository.GetAll();
            allPersons.Should().HaveCount(100);
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

        [Fact]
        public void ShouldFindPersonById()
        {
            // Arrange
            var personRepository = new InMemoryPersonRepository();
            var person1 = personRepository.Add(Testdata.Employees.CreateEmployee1());
            personRepository.Add(Testdata.Employees.CreateEmployee2());

            // Act
            var person = personRepository.FindById(person1.Id);

            // Assert
            person.Should().BeSameAs(person1);
        }

        [Fact]
        public async Task ShouldFindPersonByIdAsync()
        {
            // Arrange
            var personRepository = new InMemoryPersonRepository();
            var person1 = personRepository.Add(Testdata.Employees.CreateEmployee1());
            personRepository.Add(Testdata.Employees.CreateEmployee2());

            // Act
            var person = await personRepository.FindByIdAsync(person1.Id);

            // Assert
            person.Should().BeSameAs(person1);
        }

        [Fact]
        public void ShouldThrowWhenFindingPersonByMultipleIds()
        {
            // Arrange
            var personRepository = new InMemoryPersonRepository();
            var person1 = personRepository.Add(Testdata.Employees.CreateEmployee1());
            var person2 = personRepository.Add(Testdata.Employees.CreateEmployee2());

            // Act
            var action = () => personRepository.FindById(person1.Id, person2.Id);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task ShouldThrowWhenFindingPersonByMultipleIdsAsync()
        {
            // Arrange
            var personRepository = new InMemoryPersonRepository();
            var person1 = personRepository.Add(Testdata.Employees.CreateEmployee1());
            var person2 = personRepository.Add(Testdata.Employees.CreateEmployee2());

            // Act
            var action = async () => await personRepository.FindByIdAsync(person1.Id, person2.Id);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }
    }

    public class InMemoryPersonRepository : InMemoryRepository<Person>, IPersonRepository
    {
    }
}
