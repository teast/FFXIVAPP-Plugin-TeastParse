namespace FFXIVAPP.Plugin.TeastParse.Repositories
{
    public interface IRepositoryFactory
    {
        IRepository Create(string connectionString);
    }

    public class RepositoryFactory : IRepositoryFactory
    {
        public IRepository Create(string connectionString)
        {
            return new Repository(connectionString);
        }
    }
}