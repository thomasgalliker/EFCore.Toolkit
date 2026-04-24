using AwesomeAssertions;
using ToolkitSample.Model;

namespace EFCore.Toolkit.Tests.Extensions
{
    internal static class AssertionExtensions
    {
        internal static void ShouldBeEquivalentTo(this Employee subject, Employee expected)
        {
            subject.Should().BeEquivalentTo(expected, options => options.IncludingAllRuntimeProperties()
                                                                             .IgnoringCyclicReferences()
                                                                             .Excluding(e => e.Id)
                                                                             .Excluding(e => e.Department)
                                                                             .Excluding(e => e.DepartmentId)
                                                                             .Excluding(e => e.CountryId)
                                                                             .Excluding(e => e.CreatedDate)
                                                                             .Excluding(e => e.UpdatedDate)
                                                                             .Excluding(e => e.RowVersion));
        }

        internal static void ShouldBeEquivalentTo(this Student subject, Student expected)
        {
            subject.Should().BeEquivalentTo(expected, options => options.IncludingAllRuntimeProperties()
                                                                             .IgnoringCyclicReferences()
                                                                             .Excluding(e => e.Id)
                                                                             .Excluding(e => e.CountryId)
                                                                             .Excluding(e => e.CreatedDate)
                                                                             .Excluding(e => e.UpdatedDate)
                                                                             .Excluding(e => e.RowVersion));
        }

        internal static void ShouldBeEquivalentTo(this Department subject, Department expected)
        {
            subject.Should().BeEquivalentTo(expected, options => options.IncludingAllRuntimeProperties()
                                                                             .IgnoringCyclicReferences()
                                                                             .Excluding(e => e.Id)
                                                                             .Excluding(e => e.Employees)
                                                                             .Excluding(e => e.Leader)
                                                                             .Excluding(e => e.LeaderId)
                                                                             .Excluding(e => e.RowVersion));
        }
    }
}
