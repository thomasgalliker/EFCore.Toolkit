namespace EFCore.Toolkit
{
    public interface IUserContext<out TKey>
    {
        public TKey UserId { get; }
    }
}