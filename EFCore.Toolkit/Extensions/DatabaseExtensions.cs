using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFCore.Toolkit.Extensions
{
    public static class DatabaseExtensions
    {
        public static void KillConnectionsToTheDatabase(this DatabaseFacade database)
        {
            try
            {
                var dbConnection = database.GetDbConnection();
                var databaseName = dbConnection.Database;
                const string sqlFormat = @"
                     USE master; 

                     DECLARE @databaseName VARCHAR(50);
                     SET @databaseName = '{0}';

                     declare @kill varchar(8000) = '';
                     select @kill=@kill+'kill '+convert(varchar(5),spid)+';'
                     from master..sysprocesses 
                     where dbid=db_id(@databaseName);

                     exec (@kill);";

                var sql = string.Format(sqlFormat, databaseName);

                using (var command = dbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    command.Connection.Open();

                    command.ExecuteNonQuery();

                    command.Connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

