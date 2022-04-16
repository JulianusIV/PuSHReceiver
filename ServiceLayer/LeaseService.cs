using DataLayer.JSONObject;
using ServiceLayer.DataService;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Timers;

namespace ServiceLayer.Service
{
    public class LeaseService
    {
        #region Singleton
        private static LeaseService _instance;
        private static readonly object _instanceLock = new object();
        public static LeaseService Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance is null)
                        _instance = new LeaseService();
                    return _instance;
                }
            }
        }
        #endregion

        private static readonly object _padlock = new object();
        private static Dictionary<ulong, Timer> LeaseTimers { get; } = new Dictionary<ulong, Timer>();

        private readonly ISubscriptionService subscriptionService;
        public LeaseService()
        {
            subscriptionService = new SubscriptionService(new TopicDataService());
        }

        public void RegisterLease(DataSub dataSub, int leaseTime)
        {
            lock (_padlock)
            {
                Console.WriteLine($"Scheduling lease renewal for topic {dataSub.TopicID} in {leaseTime} seconds ({TimeSpan.FromSeconds(leaseTime).TotalDays} days)");

                var timer = GetTimer(dataSub);
                timer.Stop();
                timer.Interval = TimeSpan.FromSeconds(leaseTime).TotalMilliseconds;
                timer.AutoReset = false;
                timer.Start();
            }
        }

        private Timer GetTimer(DataSub dataSub)
        {
            if (LeaseTimers.ContainsKey(dataSub.TopicID))
                return LeaseTimers[dataSub.TopicID];

            var timer = new Timer();
            timer.Elapsed += async (sender, e) => await subscriptionService.SubscribeAsync(dataSub);
            LeaseTimers.Add(dataSub.TopicID, timer);
            return timer;
        }
    }
}
