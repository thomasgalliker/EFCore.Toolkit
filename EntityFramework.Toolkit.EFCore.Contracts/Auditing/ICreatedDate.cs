using System;

namespace EntityFramework.Toolkit.EFCore.Contracts.Auditing
{
    public interface ICreatedDate
    {
        DateTime CreatedDate { get; set; }
    }
}
