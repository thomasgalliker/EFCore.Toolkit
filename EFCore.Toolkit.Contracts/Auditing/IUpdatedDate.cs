using System;

namespace EFCore.Toolkit.Abstractions.Auditing
{
    public interface IUpdatedDate
    {
        DateTime? UpdatedDate { get; set; }
    }
}