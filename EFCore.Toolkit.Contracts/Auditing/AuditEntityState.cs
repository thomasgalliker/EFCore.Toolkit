using System;

namespace EFCore.Toolkit.Contracts.Auditing
{
    /// <summary>
    ///     Describes the state of an audit entity.
    /// </summary>
    [Flags]
    public enum AuditEntityState
    {
        Added = 4,
        Deleted = 8,
        Modified = 16
    }
}