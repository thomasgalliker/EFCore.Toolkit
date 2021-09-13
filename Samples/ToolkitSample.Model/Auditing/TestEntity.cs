using System;
using EFCore.Toolkit.Abstractions.Auditing;

namespace ToolkitSample.Model.Auditing
{
    public class TestEntity : IUpdatedDate
    {
        public int TestEntityId { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdateUser { get; set; }
    }
}
