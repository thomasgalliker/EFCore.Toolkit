namespace EFCore.Toolkit
{
    public interface IUserContext<out TKey>
    {
        TKey GetCurrentUserId();
    }
}