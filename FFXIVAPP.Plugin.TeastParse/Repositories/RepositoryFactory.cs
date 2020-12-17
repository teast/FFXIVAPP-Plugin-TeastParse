namespace FFXIVAPP.Plugin.TeastParse.Repositories
{
    /// <summary>
    /// Helper to create an repository
    /// </summary>
    public interface IRepositoryFactory
    {
        IRepository Create(string connectionString, bool readOnly = false);
    }

    /// <summary>
    /// Creates an repository 
    /// </summary>
    public class RepositoryFactory : IRepositoryFactory
    {
        public IRepository Create(string connectionString, bool readOnly = false)
        {
            if (readOnly)
                return new RepositoryReadOnly(connectionString);
                
            return new Repository(connectionString);
        }
    }
}