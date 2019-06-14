namespace EFCore.Toolkit.Abstractions
{
    public interface IDeletable
    {
        bool IsDeleted { get; set; }
    }
}