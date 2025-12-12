using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

using EFCore.Toolkit;

using ToolkitSample.Model;

namespace ToolkitSample.DataAccess.Seed
{
    internal sealed class ApplicationSettingDataSeed : DataSeedBase<ApplicationSetting>
    {
        public override Expression<Func<ApplicationSetting, object>> AddOrUpdateExpression
        {
            get => applicationSetting => applicationSetting.Id;
        }

        public override IEnumerable<ApplicationSetting> GetAll()
        {
            yield return new ApplicationSetting { Path = "/../../TestForSeed" };
        }
    }
}
