namespace EFCore.Toolkit.Abstractions
{
    public interface IDbConnection
    {
        string Name { get; }

        string ConnectionString { get; }

        bool LazyLoadingEnabled { get; set; }

        bool ForceInitialize { get; }
    }
}
