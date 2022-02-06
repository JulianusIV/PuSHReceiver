using System;
using System.Collections.Generic;

namespace PubSubHubBubReciever.JSONObjects
{
    public class Leases
    {
        public List<LeaseSub> Subs { get; set; }
    }

    public class LeaseSub
    {
        public long TopicID { get; set; }
        public DateTime LastLease { get; set; }
        public int LeaseTime { get; set; }
        public bool Subscribed { get; set; }
    }

}
