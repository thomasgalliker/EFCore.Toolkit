using System.Diagnostics;
using EFCore.Toolkit.Abstractions;

namespace ToolkitSample.Model
{
    [DebuggerDisplay("Country: Id={Id}, Name={Name}")]
    public class Country : IDeletable
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool IsDeleted { get; set; }
    }
}