using Contracts.Repositories;
using Contracts.Service;
using Microsoft.Extensions.Logging;

namespace Service
{
    public class ShutdownService : IShutdownService
    {
        private bool _isShutdownRequested;
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<ShutdownService> _logger;
        private readonly ILeaseRepository _leaseRepository;

        public ShutdownService(ISubscriptionService subscriptionService, ILogger<ShutdownService> logger, ILeaseRepository leaseRepository)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
            _leaseRepository = leaseRepository;
        }

        public void Shutdown()
        {
            //if any leases are subscribed, unsubscribe, then wait for cancellation token
            if (_leaseRepository.CountSubscribedLeases() > 0)
            {
                _isShutdownRequested = true;
                _subscriptionService.UnsubscribeAll();
                var token = _tokenSource.Token;
                token.WaitHandle.WaitOne(TimeSpan.FromMinutes(3));
                if (token.IsCancellationRequested)
                    _logger.LogDebug("Unsubscribe successful, continuing graceful shutdown.");
                else
                    _logger.LogWarning("Timeout, shutting down without or with partial unsubscribe.");
            }
            else
                _logger.LogDebug("No leases subscribed, continuing graceful shutdown.");
        }

        public void TriggerAllSubsUnsubscribed()
        {
            if (_isShutdownRequested)
                _tokenSource.Cancel();
        }
    }
}
