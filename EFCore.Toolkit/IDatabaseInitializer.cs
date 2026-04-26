namespace EFCore.Toolkit
{
    public interface IDatabaseInitializer
    {
        void Initialize(DbContextBase context, bool force);
    }
}