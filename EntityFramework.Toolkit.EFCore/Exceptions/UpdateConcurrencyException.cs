using System;

namespace EntityFramework.Toolkit.EFCore.Exceptions
{
    public class UpdateConcurrencyException : Exception
    {
        public UpdateConcurrencyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
