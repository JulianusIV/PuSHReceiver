using Contracts.DbContext;
using Models;
using PluginLibrary.PluginRepositories;

namespace Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly IDbContext _dbContext;
        private readonly SemaphoreSlim _semaphore = new(1);

        public LogRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void CreateLogEntry(Log log)
        {
            _semaphore.Wait();

            try
            {
                if (_dbContext.Logs.Count() > 99)
                    _dbContext.Logs.Remove(_dbContext.Logs.MinBy(x => x.Timestamp)!);
                
                _dbContext.Logs.Add(log);
                _dbContext.SaveChanges();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
