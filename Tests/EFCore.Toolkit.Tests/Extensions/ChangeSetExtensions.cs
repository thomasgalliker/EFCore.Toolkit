using System.Text;
using EFCore.Toolkit.Abstractions;
using Xunit.Sdk;

namespace EFCore.Toolkit.Tests.Extensions
{
    internal static class ChangeSetExtensions
    {
        internal static void Assert(this ChangeSet changeSet, int expectedNumberOfAdded, int expectedNumberOfModified, int expectedNumberOfDeleted)
        {
            var numberOfAdded = changeSet.Changes.Count(c => c.State == ChangeState.Added);
            var numberOfModified = changeSet.Changes.Count(c => c.State == ChangeState.Modified);
            var numberOfDeleted = changeSet.Changes.Count(c => c.State == ChangeState.Deleted);

            if (numberOfAdded == expectedNumberOfAdded &&
                numberOfModified == expectedNumberOfModified &&
                numberOfDeleted == expectedNumberOfDeleted)
            {
                return;
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Expected ChangeSet to contain: ");
            stringBuilder.AppendLine($" - Added = {expectedNumberOfAdded}");
            stringBuilder.AppendLine($" - Modified = {expectedNumberOfModified}");
            stringBuilder.AppendLine($" - Deleted = {expectedNumberOfDeleted}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("but found:");
            stringBuilder.AppendLine($" - Added = {numberOfAdded}");
            stringBuilder.AppendLine($" - Modified = {numberOfModified}");
            stringBuilder.AppendLine($" - Deleted = {numberOfDeleted}");

            throw new XunitException(stringBuilder.ToString());
        }
    }
}
