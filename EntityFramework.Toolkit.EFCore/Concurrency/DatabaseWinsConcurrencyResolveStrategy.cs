namespace EntityFramework.Toolkit.EFCore.Concurrency
{
    public sealed class DatabaseWinsConcurrencyResolveStrategy : IConcurrencyResolveStrategy
    {
        public object ResolveConcurrencyException(object conflictingEntity, object databaseEntity)
        {
            return databaseEntity;
        }
    }
}
