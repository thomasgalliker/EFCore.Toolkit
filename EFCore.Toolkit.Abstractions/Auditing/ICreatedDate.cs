using System;

namespace EFCore.Toolkit.Abstractions.Auditing
{
    public interface ICreatedDate
    {
        DateTime CreatedDate { get; set; }
    }
}
