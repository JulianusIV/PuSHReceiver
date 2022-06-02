using Data.JSONObjects;
using Plugins.Interfaces;
using PubSubHubBubReciever;
using Services;
using System.Runtime.CompilerServices;
using Timer = System.Timers.Timer;

namespace ServiceLayer
{
    public class LeaseService : ILeaseService
    {
        private static Dictionary<ulong, Timer> _leaseTimers { get; } = new();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RegisterLease(DataSub dataSub, int leaseTime)
        {
            Console.WriteLine($"Scheduling lease renewal for topic {dataSub.TopicID} in {leaseTime} seconds ({TimeSpan.FromSeconds(leaseTime).TotalDays} days)");

            var timer = GetTimer(dataSub);
            timer.Stop();
            timer.Interval = TimeSpan.FromSeconds(leaseTime).TotalMilliseconds;
            timer.AutoReset = false;
            timer.Start();
        }

        private Timer GetTimer(DataSub dataSub)
        {
            if (_leaseTimers.ContainsKey(dataSub.TopicID))
                return _leaseTimers[dataSub.TopicID];

            var timer = new Timer();
            timer.Elapsed += async (sender, e)
                => await Runtime.Instance.PluginLoader.ResolvePlugin<IConsumerPlugin>(dataSub.FeedConsumer)
                    .SubscribeAsync(dataSub);
            _leaseTimers.Add(dataSub.TopicID, timer);
            return timer;
        }
    }
}
