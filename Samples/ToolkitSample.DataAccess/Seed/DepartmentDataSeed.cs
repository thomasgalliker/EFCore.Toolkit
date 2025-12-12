using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using EFCore.Toolkit;

using ToolkitSample.Model;

namespace ToolkitSample.DataAccess.Seed
{
    internal sealed class DepartmentDataSeed : DataSeedBase<Department>
    {
        public override Expression<Func<Department, object>> AddOrUpdateExpression
        {
            get => department => department.Name;
        }

        public override IEnumerable<Department> GetAll()
        {
            yield return new Department { Name = "Administration" };
            yield return new Department { Name = "Human Resources" };
        }
    }
}
