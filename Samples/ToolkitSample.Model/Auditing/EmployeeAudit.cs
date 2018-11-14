using EFCore.Toolkit.Contracts.Auditing;

namespace ToolkitSample.Model.Auditing
{
    public class EmployeeAudit : AuditEntity
    {
        private int id;

        public int Id
        {
            get => this.id;
            set => this.id = value;
        }

        public virtual string FirstName { get; set; }

        public virtual string LastName { get; set; }
    }
}