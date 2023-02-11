using Contracts.DbContext;

namespace Repositories
{
    public class UserRepository
    {
        private readonly IDbContext _dbContext;
        private readonly SemaphoreSlim _semaphore = new(1);

        public UserRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}