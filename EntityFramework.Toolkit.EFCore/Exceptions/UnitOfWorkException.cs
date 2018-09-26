using System;

namespace EntityFramework.Toolkit.EFCore.Exceptions
{
    public class UnitOfWorkException : Exception
    {
        public UnitOfWorkException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
