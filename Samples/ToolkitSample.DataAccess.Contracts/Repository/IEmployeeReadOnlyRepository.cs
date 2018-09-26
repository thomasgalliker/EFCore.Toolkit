using EntityFramework.Toolkit.EFCore;
using EntityFramework.Toolkit.EFCore.Contracts;
using ToolkitSample.Model;

namespace ToolkitSample.DataAccess.Contracts.Repository
{
    public interface IEmployeeReadOnlyRepository : IReadOnlyRepository<Employee>
    {
    }
}