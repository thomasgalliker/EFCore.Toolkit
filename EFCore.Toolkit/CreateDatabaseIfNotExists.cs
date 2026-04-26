namespace EFCore.Toolkit
{
    public class CreateDatabaseIfNotExists : IDatabaseInitializer
    {
        public void Initialize(DbContextBase context, bool force)
        {
            context.Database.EnsureCreated();
        }
    }
}