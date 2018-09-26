using System;

namespace EntityFramework.Toolkit.EFCore.Contracts.Auditing
{
    public interface IUpdatedDate
    {
        DateTime? UpdatedDate { get; set; }
    }
}