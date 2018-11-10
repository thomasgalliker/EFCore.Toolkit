using System;

namespace EFCore.Toolkit.Contracts.Auditing
{
    public interface IUpdatedDate
    {
        DateTime? UpdatedDate { get; set; }
    }
}