namespace EFCore.Toolkit
{
    public class DropCreateDatabaseAlways : IDatabaseInitializer
    {
        public void Initialize(DbContextBase context, bool force)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}