using Contracts;
using Contracts.Repositories;
using Contracts.Service;
using PluginLibrary.Interfaces;

namespace Service
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ILeaseRepository _leaseRepository;
        private readonly IPluginManager _pluginManager;
        private readonly ILeaseService _leaseService;
        public SubscriptionService(ILeaseRepository leaseRepository, IPluginManager pluginManager, ILeaseService leaseService)
        {
            _leaseRepository = leaseRepository;
            _pluginManager = pluginManager;
            _leaseService = leaseService;
        }

        public async void SubscribeAll()
        {
            //fetch leases to subscribe to from db and subscribe
            foreach (var lease in _leaseRepository.GetActiveExpiredLeases())
                await _pluginManager.ResolvePlugin<IConsumerPlugin>(lease.Consumer)
                    .SubscribeAsync(lease);

            //fetch leases that have not yet expired and register renewal
            foreach (var lease in _leaseRepository.GetRunningLeases())
                _leaseService.RegisterLease(
                    lease,
                    (int)(lease.LastLease + TimeSpan.FromSeconds(lease.LeaseTime) - DateTime.Now).TotalSeconds);
        }

        public async void UnsubscribeAll()
        {
            //fetch leases that are currently subscribed and send unsubscribe
            foreach (var lease in _leaseRepository.GetSubscribedLeases())
                await _pluginManager.ResolvePlugin<IConsumerPlugin>(lease.Consumer)
                    .SubscribeAsync(lease, false);
        }
    }
}
