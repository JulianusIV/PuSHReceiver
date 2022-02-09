using PubSubHubBubReciever.JSONObject;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubSubHubBubReciever.DataService.Interface
{
    internal interface ITopicDataService
    {
        internal DataSub GetDataSub(Guid id);
        internal LeaseSub GetLeaseSub(Guid id);
        internal bool VerifyAdminToken(string token);
        internal string GetCallback(Guid id);
        internal bool AddTopic(DataSub dataSub, LeaseSub leaseSub);
        internal bool UpdateTopic(DataSub dataSub, LeaseSub leaseSub);
        internal bool DeleteTopic(DataSub dataSub, LeaseSub leaseSub);
        internal (List<DataSub>, List<LeaseSub>) GetExpiredAndRunningSubs();
        internal void UpdateLease(Guid id, bool subscribe, int leaseTime = 0);
        internal int CountSubbedTopics();
    }
}
