﻿namespace EFCore.Toolkit.Testing
{
    public class RandomizedDbConnection : DbConnection
    {
        public RandomizedDbConnection(string connectionString) : base(connectionString.RandomizeDatabaseName())
        {
        }
    }
}