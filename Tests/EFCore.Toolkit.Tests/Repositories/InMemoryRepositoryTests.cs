using System.Linq;
using System.Threading.Tasks;
using EFCore.Toolkit.Tests.Stubs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ToolkitSample.DataAccess.Contracts.Repository;
using ToolkitSample.Model;
using Xunit;
using Xunit.Abstractions;

namespace EFCore.Toolkit.Tests.Repository
{
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
    }

    public class InMemoryPersonRepository : InMemoryRepository<Person>, IPersonRepository
    {
    }
}
