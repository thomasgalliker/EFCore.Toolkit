using System;

namespace EFCore.Toolkit.Contracts.Auditing
{
    public interface ICreatedDate
    {
        DateTime CreatedDate { get; set; }
    }
}
