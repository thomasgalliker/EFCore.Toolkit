using System.Diagnostics;

namespace ToolkitSample.Model
{
    [DebuggerDisplay("Department: Id={Id}, Name={Name}, Employees={this.Employees.Count}")]
    public class Department
    {
        public Department()
        {
            this.Employees = new HashSet<Employee>();
            this.RowVersion = Array.Empty<byte>();
        }

        public int Id { get; set; }

        public string? Name { get; set; }

        public int? LeaderId { get; set; }

        public Person? Leader { get; set; }

        public ICollection<Employee> Employees { get; set; }

        public byte[] RowVersion { get; set; }
    }
}