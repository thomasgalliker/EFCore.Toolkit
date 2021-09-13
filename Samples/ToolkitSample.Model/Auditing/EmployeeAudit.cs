using EFCore.Toolkit.Abstractions.Auditing;

namespace ToolkitSample.Model.Auditing
{
    public class EmployeeAudit : AuditEntity
    {
        public int Id { get; set; }

        public virtual string FirstName { get; set; }

        public virtual string LastName { get; set; }
    }
}