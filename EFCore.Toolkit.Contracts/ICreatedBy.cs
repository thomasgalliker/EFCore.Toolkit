namespace EFCore.Toolkit.Abstractions
{
    public interface ICreatedBy<TKey>
    {
        TKey CreatedBy { get; set; }
    }
}