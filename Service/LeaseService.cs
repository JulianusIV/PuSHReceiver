using Contracts;
using Contracts.Service;
using Microsoft.Extensions.Logging;
using Models;
using PluginLibrary.Interfaces;
using System.Runtime.CompilerServices;
using Timer = System.Timers.Timer;

namespace Service
{
    public class LeaseService : ILeaseService
    {
        private readonly Dictionary<int, Timer> _timers = new();
        private readonly ILogger<LeaseService> _logger;
        private readonly IPluginManager _pluginManager;

        public LeaseService(ILogger<LeaseService> logger, IPluginManager pluginManager)
        {
            _logger = logger;
            _pluginManager = pluginManager;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RegisterLease(Lease lease, int leaseTime = 0)
        {
            //fetch leaseTime from lease object if none given
            leaseTime = leaseTime == 0 ? lease.LeaseTime : leaseTime;

            //calculate timer duration
            var leaseTimespan = TimeSpan.FromSeconds(leaseTime) - (DateTime.Now - lease.LastLease);
            _logger.LogDebug("Scheduling lease renewal for topic {TopicIc} in {LeaseSeconds} seconds ({LeaseDays} days", lease.Id, leaseTime, leaseTimespan.TotalDays);

            //fetch appropriate timer, stop if running, set properties and run
            var timer = GetTimer(lease);
            timer.Stop();
            timer.Interval = leaseTimespan.TotalMilliseconds;
            timer.AutoReset = false;
            timer.Start();
        }

        private Timer GetTimer(Lease lease)
        {
            //if timer already exists for lease return it
            if (_timers.TryGetValue(lease.Id, out Timer? value))
                return value;

            //create new timer add event handler
            var timer = new Timer();
            timer.Elapsed += async (sender, e)
                => await _pluginManager.ResolvePlugin<IConsumerPlugin>(lease.Consumer)
                    .SubscribeAsync(lease);
            _timers.Add(lease.Id, timer);
            return timer;
        }
    }
}