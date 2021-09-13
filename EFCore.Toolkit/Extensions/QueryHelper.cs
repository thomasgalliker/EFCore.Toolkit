using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Toolkit.Extensions
{
    internal static class QueryHelper
    {
        private static string GetColumnName(this MemberInfo info)
        {
            List<ColumnAttribute> list = info.GetCustomAttributes<ColumnAttribute>().ToList();
            return list.Count > 0 ? list.Single().Name : info.Name;
        }
        /// <summary>
        /// Executes raw query with parameters and maps returned values to column property names of Model provided.
        /// Not all properties are required to be present in model (if not present - null)
        /// </summary>
        public static async IAsyncEnumerable<T> ExecuteQuery<T>(this DbContext dbContext, string query, params SqlParameter[] parameters)
            where T : class, new()
        {
            using DbCommand command = dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            if (parameters != null)
            {
                foreach (SqlParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }
            await dbContext.Database.OpenConnectionAsync();
            using DbDataReader reader = await command.ExecuteReaderAsync();
            List<PropertyInfo> lstColumns = new T().GetType()
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            while (await reader.ReadAsync())
            {
                var newObject = new T();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string name = reader.GetName(i);
                    PropertyInfo prop = lstColumns.FirstOrDefault(a => a.GetColumnName().Equals(name));
                    if (prop == null)
                    {
                        continue;
                    }
                    object val = await reader.IsDBNullAsync(i) ? null : reader[i];
                    prop.SetValue(newObject, val, null);
                }
                yield return newObject;
            }
        }
    }
}
