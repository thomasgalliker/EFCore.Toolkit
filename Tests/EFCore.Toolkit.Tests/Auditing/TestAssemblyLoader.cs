using System.Reflection;
using EFCore.Toolkit.Utils;

namespace EFCore.Toolkit.Tests.Auditing
{
    /// <summary>
    /// Implementation for <see cref="IAssemblyLoader"/> which is used in tests.
    /// This is due to a limitation of Assembly.GetEntryAssembly() and Assembly.GetExecutingAssembly()
    /// when used in tests executed by dotnet test.
    /// </summary>
    public class TestAssemblyLoader : IAssemblyLoader
    {
        public Assembly GetEntryAssembly()
        {
            return Assembly.Load("EFCore.Toolkit.Tests");
        }

        public Assembly GetExecutingAssembly()
        {
            return Assembly.Load("EFCore.Toolkit");
        }
    }
}