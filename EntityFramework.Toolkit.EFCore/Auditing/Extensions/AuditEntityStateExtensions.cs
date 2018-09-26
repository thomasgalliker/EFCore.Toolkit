using System;
using EntityFramework.Toolkit.EFCore.Contracts.Auditing;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework.Toolkit.EFCore.Auditing.Extensions
{
    public static class AuditEntityStateExtensions
    {
        public static AuditEntityState ToAuditEntityState(this EntityState entityState)
        {
            switch (entityState)
            {
                case EntityState.Added:
                    return AuditEntityState.Added;
                case EntityState.Modified:
                    return AuditEntityState.Modified;
                case EntityState.Deleted:
                    return AuditEntityState.Deleted;

                default:
                    throw new NotSupportedException($"EntityState.{entityState} is not supported.");
            }
        }
    }
}