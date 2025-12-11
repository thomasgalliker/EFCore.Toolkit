using System.Linq.Expressions;

namespace EFCore.Toolkit.Abstractions
{
    public interface IDataSeed
    {
        void Seed(IContext context);
    }
}
