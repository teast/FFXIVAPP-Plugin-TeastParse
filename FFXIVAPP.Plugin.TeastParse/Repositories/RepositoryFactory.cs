using FFXIVAPP.Plugin.TeastParse.Factories;

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
        private IActionFactory _actionFactory;

        public RepositoryFactory(IActionFactory actionFactory)
        {
            _actionFactory = actionFactory;
        }
        
        public IRepository Create(string connectionString, bool readOnly = false)
        {
            if (readOnly)
                return new RepositoryReadOnly(connectionString, _actionFactory);
                
            return new Repository(connectionString, _actionFactory);
        }
    }
}