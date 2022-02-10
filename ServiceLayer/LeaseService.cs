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

        private static Dictionary<Guid, Timer> LeaseTimers { get; } = new Dictionary<Guid, Timer>();

        private readonly ISubscriptionService subscriptionService;
        public LeaseService()
        {
            subscriptionService = new SubscriptionService(new TopicDataService());
        }

        public void RegisterLease(DataSub dataSub, int leaseTime)
        {
            Console.WriteLine($"Scheduling lease renewal for topic {dataSub.TopicID:N} in {leaseTime} seconds ({TimeSpan.FromSeconds(leaseTime).TotalDays} days)");
            if (!LeaseTimers.ContainsKey(dataSub.TopicID))
                LeaseTimers.Add(dataSub.TopicID, new Timer());

            LeaseTimers[dataSub.TopicID].Stop();
            LeaseTimers[dataSub.TopicID].Interval = TimeSpan.FromSeconds(leaseTime).TotalMilliseconds;
            LeaseTimers[dataSub.TopicID].AutoReset = false;
            LeaseTimers[dataSub.TopicID].Elapsed += async (sender, e) => await subscriptionService.SubscribeAsync(dataSub);
            LeaseTimers[dataSub.TopicID].Start();
        }
    }
}
