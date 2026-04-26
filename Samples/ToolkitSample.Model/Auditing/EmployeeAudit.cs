using EFCore.Toolkit.Abstractions.Auditing;

namespace ToolkitSample.Model.Auditing
{
    public class EmployeeAudit : AuditEntity
    {
        public int Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
    }
}