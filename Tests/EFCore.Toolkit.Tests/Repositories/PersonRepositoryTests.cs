using EFCore.Toolkit.Abstractions;
using EFCore.Toolkit.Testing;
using EFCore.Toolkit.Tests.Stubs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ToolkitSample.DataAccess;
using ToolkitSample.DataAccess.Context;
using ToolkitSample.DataAccess.Contracts.Repository;
using ToolkitSample.DataAccess.Repository;
using ToolkitSample.DataAccess.Seed;
using ToolkitSample.Model;
using Xunit;
using Xunit.Abstractions;


namespace EFCore.Toolkit.Tests.Repositories
{
    [Collection("DbContextTests")]
    public class PersonRepositoryTests : ContextTestBase<EmployeeContext>
    {
        public PersonRepositoryTests(ITestOutputHelper testOutputHelper)
            : base(dbContextOptions: EmployeeContextTestDbConnection.CreateDbContextOptions<EmployeeContext>(),
                  databaseInitializer: new EmployeeContextDatabaseInitializer(new List<IDataSeed> { new CountryDataSeed() }),
                  log: testOutputHelper.WriteLine)
        {
        }

        [Fact]
        public void ShouldIncludeAllNavigationPropertiesOfBaseEntity()
        {
            // Arrange
            var department = Testdata.Departments.CreateFacultyOfLaw();

            var employee1 = Testdata.Employees.CreateEmployee1();
            employee1.Department = department;

            var employee2 = Testdata.Employees.CreateEmployee2();
            employee2.Department = department;

            var student1 = Testdata.Students.CreateStudent1();
            var student2 = Testdata.Students.CreateStudent2();

            using (IPersonRepository personRepository = new PersonRepository(this.CreateContext()))
            {
                personRepository.Add(employee1);
                personRepository.Add(employee2);
                personRepository.Add(student1);
                personRepository.Add(student2);
                personRepository.Save();
            }

            // Act
            List<Person> returnedPersons;
            using (IPersonRepository personRepository = new PersonRepository(this.CreateContext()))
            {
                returnedPersons = personRepository.Get()
                    .Include(p => p.Country)
                    .ToList();
            }

            // Assert
            returnedPersons.Should().NotBeNull();
            returnedPersons.Should().HaveCount(4);
        }

        [Fact(Skip = "not yet implemented")]
        public void ShouldIncludeAllNavigationPropertiesOfDerivedEntities()
        {
            // Arrange
            var department = Testdata.Departments.CreateFacultyOfLaw();

            var employee1 = Testdata.Employees.CreateEmployee1();
            employee1.Department = department;
            employee1.CountryId = "CH";

            var employee2 = Testdata.Employees.CreateEmployee2();
            employee2.Department = department;
            employee2.CountryId = "CH";

            var student1 = Testdata.Students.CreateStudent1();
            student1.CountryId = "CH";

            var student2 = Testdata.Students.CreateStudent2();
            student2.CountryId = "CH";

            using (IPersonRepository personRepository = new PersonRepository(this.CreateContext()))
            {
                personRepository.Add(employee1);
                personRepository.Add(employee2);
                personRepository.Add(student1);
                personRepository.Add(student2);
                personRepository.Save();
            }

            // Act
            IEnumerable<Person> returnedPersons;
            using (IPersonRepository personRepository = new PersonRepository(this.CreateContext()))
            {
                returnedPersons = personRepository.Get().ToList();
            }

            // Assert
            using (IPersonRepository personRepository = new PersonRepository(this.CreateContext()))
            {
                var expectedPersons = personRepository.Get().OfType<Employee>()
                    .Include(e => e.Country)
                    .Include(e => e.Department).As<IEnumerable<Person>>().ToList()
                  .Union(personRepository.Get().OfType<Student>()
                  .Include(s => s.Country).As<IEnumerable<Person>>().ToList()).ToList();

                returnedPersons.Should().AllBeEquivalentTo(expectedPersons);
            }

            returnedPersons.Should().NotBeNull();
            returnedPersons.Should().HaveCount(4);

            returnedPersons.OfType<Employee>().Should().HaveCount(2);
            returnedPersons.OfType<Employee>().All(e => e.Department != null).Should().BeTrue();
            returnedPersons.OfType<Employee>().All(e => e.Country != null).Should().BeTrue();

            returnedPersons.OfType<Student>().Should().HaveCount(2);
            returnedPersons.OfType<Student>().All(e => e.Country != null).Should().BeTrue();
        }
    }
}