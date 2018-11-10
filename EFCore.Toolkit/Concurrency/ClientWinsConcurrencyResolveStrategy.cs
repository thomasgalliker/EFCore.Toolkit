namespace EFCore.Toolkit.Concurrency
{
    public sealed class ClientWinsConcurrencyResolveStrategy : IConcurrencyResolveStrategy
    {
        public object ResolveConcurrencyException(object conflictingEntity, object databaseEntity)
        {
            return conflictingEntity;
        }
    }
}
