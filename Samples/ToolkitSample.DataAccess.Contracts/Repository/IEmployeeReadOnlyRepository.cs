using EFCore.Toolkit;
using EFCore.Toolkit.Abstractions;
using ToolkitSample.Model;

namespace ToolkitSample.DataAccess.Contracts.Repository
{
    public interface IEmployeeReadOnlyRepository : IReadOnlyRepository<Employee>
    {
    }
}