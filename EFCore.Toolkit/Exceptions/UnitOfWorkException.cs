using System;

namespace EFCore.Toolkit.Exceptions
{
    public class UnitOfWorkException : Exception
    {
        public UnitOfWorkException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
