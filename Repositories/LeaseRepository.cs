using Contracts.DbContext;
using Contracts.Repositories;
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
                //AsEnumerable because EFCore sometimes cant translate TimeSpan.FromSeconds
                return _dbContext.Leases.AsEnumerable().Where(x => ((x.LastLease + TimeSpan.FromSeconds(x.LeaseTime) < DateTime.Now) || !x.Subscribed) && x.Active);
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
                //AsEnumerable because EFCore sometimes cant translate TimeSpan.FromSeconds
                return _dbContext.Leases.AsEnumerable().Where(x => x.LastLease + TimeSpan.FromSeconds(x.LeaseTime) >= DateTime.Now);
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

        public IEnumerable<Lease> GetLeasesByUser(User owner)
        {
            _semaphore.Wait();

            try
            {
                return _dbContext.Leases.Where(x => x.Owner != null && x.Owner.Id == owner.Id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void CreateLease(Lease lease)
        {
            _semaphore.Wait();

            try
            {
                //fetch owner from DB to not create duplicate user
                lease.Owner = lease.Owner is null ? null : _dbContext.Users.First(x => x.Id == lease.Owner.Id);
                _dbContext.Leases.Add(lease);
                _dbContext.SaveChanges();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void UpdateLease(Lease lease)
        {
            _semaphore.Wait();

            try
            {
                var old = _dbContext.Leases.First(x => x.Id == lease.Id);

                old.DisplayName = lease.DisplayName;
                old.TopicUrl = lease.TopicUrl;
                old.HubUrl = lease.HubUrl;
                old.Active = lease.Active;
                old.Publisher = lease.Publisher;
                old.Consumer = lease.Consumer;
                old.PublisherData = lease.PublisherData;
                old.ConsumerData = lease.ConsumerData;

                _dbContext.SaveChanges();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void DeleteLease(int id)
        {
            _semaphore.Wait();

            try
            {
                _dbContext.Leases.Remove(_dbContext.Leases.First(x => x.Id == id));
                _dbContext.SaveChanges();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}