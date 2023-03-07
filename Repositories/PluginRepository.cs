using Contracts.DbContext;
using Models;
using PluginLibrary.PluginRepositories;

namespace Repositories
{
    public class PluginRepository : IPluginRepository
    {
        private readonly IDbContext _dbContext;
        private readonly SemaphoreSlim _semaphore = new(1);

        public PluginRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void SaveData(Lease lease)
        {
            //wait for resource to be free
            _semaphore.Wait();

            try
            {
                var dbLease = _dbContext.Leases.First(x => x.Id == lease.Id);

                dbLease.Active = lease.Active;
                dbLease.Consumer = lease.Consumer;
                dbLease.ConsumerData = lease.ConsumerData;
                dbLease.DisplayName = lease.DisplayName;
                dbLease.HubUrl = lease.HubUrl;
                dbLease.LastLease = lease.LastLease;
                dbLease.LeaseTime = lease.LeaseTime;
                dbLease.Owner = lease.Owner;
                dbLease.Publisher = lease.Publisher;
                dbLease.PublisherData = lease.PublisherData;
                dbLease.Subscribed = lease.Subscribed;
                dbLease.TopicUrl = lease.TopicUrl;

                _dbContext.SaveChanges();
            }
            finally
            {
                //release resource
                _semaphore.Release();
            }
        }
    }
}
