using DataLayer.JSONObject;
using System;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ITopicDataService
    {
        public DataSub GetDataSub(Guid id);
        public LeaseSub GetLeaseSub(Guid id);
        public bool VerifyAdminToken(string token);
        public string GetCallback(Guid id);
        public bool AddTopic(DataSub dataSub, LeaseSub leaseSub);
        public bool UpdateTopic(DataSub dataSub, LeaseSub leaseSub);
        public bool DeleteTopic(DataSub dataSub, LeaseSub leaseSub);
        public (List<DataSub>, List<LeaseSub>) GetExpiredAndRunningSubs();
        public void UpdateLease(Guid id, bool subscribe, int leaseTime = 0);
        public int CountSubbedTopics();
        public List<DataSub> GetSubbedTopics();
    }
}
