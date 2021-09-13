using System;
using System.Reflection;

namespace EFCore.Toolkit.Utils
{
    public interface IAssemblyLoader
    {
        Assembly GetEntryAssembly();
    }

    public class AssemblyLoader : IAssemblyLoader
    {
        static AssemblyLoader()
        {
            Current = new AssemblyLoader();
        }

        public static IAssemblyLoader Current { get; internal set; }

        public Assembly GetEntryAssembly()
        {
            return Assembly.GetEntryAssembly();
        }
    }
}
