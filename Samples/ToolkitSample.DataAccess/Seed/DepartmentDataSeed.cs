using System.Linq.Expressions;

using EFCore.Toolkit;

using ToolkitSample.Model;

namespace ToolkitSample.DataAccess.Seed
{
    internal sealed class DepartmentDataSeed : DataSeedBase<Department>
    {
        public override Expression<Func<Department, object?>> AddOrUpdateExpression => d => d.Name;

        public override IEnumerable<Department> GetAll()
        {
            yield return new Department { Id = 0, Name = "Administration" };
            yield return new Department { Id = 0, Name = "Human Resources" };
            //yield return new Department { Id = 0, Name = "Test", Description = $"Used for AddOrUpdate testing @ {DateTime.Now}"};
        }
    }
}