using System;
using System.Collections.Generic;

namespace DataLayer.JSONObject
{
    public class Leases
    {
        public List<LeaseSub> Subs { get; set; }
    }

    public class LeaseSub
    {
        public ulong TopicID { get; set; }
        public DateTime LastLease { get; set; }
        public int LeaseTime { get; set; }
        public bool Subscribed { get; set; }
    }

}
