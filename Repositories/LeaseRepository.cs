using Contracts.DbContext;
using Contracts.Repositorys;
using Models;

namespace Repositories
{
    public class LeaseRepository : ILeaseRepository
    {
        private readonly IDbContext _dbContext;
        private readonly SemaphoreSlim _semaphore = new(1);

        public LeaseRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Lease? FindLease(int id)
        {
            //wait for the resource to be free, then lock it
            _semaphore.Wait();

            try
            {
                //query database
                return _dbContext.Leases.First(x => x.Id == id);
            }
            finally
            {
                //release resource
                _semaphore.Release();
            }
        }

        public IEnumerable<Lease> GetActiveExpiredLeases()
        {
            _semaphore.Wait();

            try
            {
                return _dbContext.Leases.Where(x => ((x.LastLease + TimeSpan.FromSeconds(x.LeaseTime) < DateTime.Now) || !x.Subscribed) && x.Active);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public IEnumerable<Lease> GetRunningLeases()
        {
            _semaphore.Wait();

            try
            {
                return _dbContext.Leases.Where(x => x.LastLease + TimeSpan.FromSeconds(x.LeaseTime) >= DateTime.Now);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public IEnumerable<Lease> GetSubscribedLeases()
        {
            _semaphore.Wait();

            try
            {
                return _dbContext.Leases.Where(_x => _x.Subscribed);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public int CountSubscribedLeases()
        {
            _semaphore.Wait();

            try
            {
                return _dbContext.Leases.Count(x => x.Subscribed);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}