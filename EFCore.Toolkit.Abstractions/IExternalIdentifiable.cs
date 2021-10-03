using System;

namespace EFCore.Toolkit.Abstractions
{
    public interface IExternalIdentifiable
    {
        Guid ExternalId { get; set; }
    }
}