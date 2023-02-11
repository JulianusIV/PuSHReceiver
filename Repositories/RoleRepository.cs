using Contracts.DbContext;

namespace Repositories
{
    public class RoleRepository
    {
        private readonly IDbContext _dbContext;
        private readonly SemaphoreSlim _semaphore = new(1);

        public RoleRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
