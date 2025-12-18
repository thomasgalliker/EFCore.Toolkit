using System.Diagnostics;

namespace ToolkitSample.Model
{
    [DebuggerDisplay("Employee: Id={Id}, FirstName={FirstName}, LastName={LastName}, Department={Department?.Name}")]
    public class Employee : Person
    {
        public int? DepartmentId { get; set; }

        public Department? Department { get; set; }

        public DateTime? EmployementDate { get; set; }

        public string? PropertyA { get; set; }

        public string? PropertyB { get; set; }

        public decimal? Salary { get; set; }
    }

    // Hint: https://msdn.microsoft.com/en-us/library/bb399739(v=vs.100).aspx
}